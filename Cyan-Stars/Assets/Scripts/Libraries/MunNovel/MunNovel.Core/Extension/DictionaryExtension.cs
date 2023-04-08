namespace System.Collections.Generic
{
    internal static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key) =>
            self.TryGetValue(key, out TValue value) ? value : default;
    }
}
