#if !NETSTANDARD2_1_OR_GREATER

using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    internal static class CollectionExtensions
    {
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> self, TKey key) =>
        //     GetValueOrDefault(self, key, default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> self, TKey key, TValue defaultValue) =>
            self.TryGetValue(key, out TValue value) ? value : defaultValue;

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (!self.ContainsKey(key))
            {
                self.Add(key, value);
                return true;
            }
            return false;
        }
    }
}

#endif
