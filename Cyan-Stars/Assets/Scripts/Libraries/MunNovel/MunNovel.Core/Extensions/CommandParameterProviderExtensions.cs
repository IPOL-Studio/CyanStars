using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MunNovel.Utils;

namespace MunNovel.Command
{
    public static class CommandParameterProviderExtensions
    {

        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="KeyNotFoundException" />
        public static object GetValue(this ICommandParameterProvider provider, string name)
        {
            ThrowUtils.ThrowNullOrEmpty(name, nameof(name));

            return provider.TryGetValue(name, out var value)
                ? value
                : throw new KeyNotFoundException($"parameter {name} not found");
        }

        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="KeyNotFoundException" />
        public static T GetValue<T>(this ICommandParameterProvider provider, string name)
        {
            ThrowUtils.ThrowNullOrEmpty(name, nameof(name));

            return provider.TryGetValue(name, out T value)
                ? value
                : throw new KeyNotFoundException($"parameter {name} not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueOrDefault<T>(this ICommandParameterProvider provider, string name, T defaultValue = default)
            => provider.TryGetValue(name, out T value) ? value : defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetValueOrDefault(this ICommandParameterProvider provider, string name)
            => provider.TryGetValue(name, out object value) ? value : default;
    }
}