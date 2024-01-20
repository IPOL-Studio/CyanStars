// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/Text/StringBuilderCache.cs

using System;
using System.Text;

namespace CyanStars.Framework.Utils
{
    internal static class StringBuilderCache
    {
        private const int DefaultCapacity = 16;
        private const int MaxBuilderSize = 8192;

        [ThreadStatic]
        private static StringBuilder cachedInstance;

        public static StringBuilder Acquire(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxBuilderSize)
            {
                StringBuilder sb = cachedInstance;
                if (sb != null && capacity <= sb.Capacity)
                {
                    cachedInstance = null;
                    sb.Clear();
                    return sb;
                }
            }

            return new StringBuilder(capacity);
        }

        public static void Release(StringBuilder sb)
        {
            if (sb.Capacity <= MaxBuilderSize)
            {
                cachedInstance = sb;
            }
        }

        public static string GetStringAndRelease(StringBuilder sb)
        {
            string result = sb.ToString();
            if (sb.Capacity <= MaxBuilderSize)
            {
                cachedInstance = sb;
            }

            return result;
        }
    }
}
