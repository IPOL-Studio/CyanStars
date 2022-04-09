using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Framework.Effect
{
    public static class EffectLoader
    {
        private static Dictionary<string, Type> effectRegistor = new Dictionary<string, Type>();

        public static bool TryGetEffectType(string key, out Type type) => effectRegistor.TryGetValue(key, out type);

        public static void Register<T>(string key) where T : IEffect
        {
            effectRegistor.Add(key, typeof(T));
        }

        public static EffectModel Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<EffectModel>(json);
        }

        public static EffectModel[] DeserializeArray(string json)
        {
            return JsonConvert.DeserializeObject<EffectModel[]>(json);
        }
    }
}