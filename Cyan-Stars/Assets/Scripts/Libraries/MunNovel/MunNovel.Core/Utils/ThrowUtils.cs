using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MunNovel.Utils
{
    internal static class ThrowUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNullOrEmpty<T>(ICollection<T> obj, string name)
        {
            _ = obj ?? throw new ArgumentNullException(name);

            if (obj.Count == 0)
                throw new ArgumentException("Collection is empty", name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowNullOrEmpty(string str, string name)
        {
            _ = str ?? throw new ArgumentNullException(name);

            if (str.Length == 0)
                throw new ArgumentException("String is empty", name);
        }
    }
}