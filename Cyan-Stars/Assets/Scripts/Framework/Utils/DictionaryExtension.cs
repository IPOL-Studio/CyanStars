using System.Collections.Generic;

namespace CyanStars.Framework.Utils
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key)
        {
            return self.TryGetValue(key, out TValue value) ? value : default;
        }
    }
}
