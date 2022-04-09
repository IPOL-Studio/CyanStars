using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CyanStars.Framework.Effect
{
    [JsonConverter(typeof(EffectConverter))]
    public sealed class EffectModel
    {
        public readonly string key;
        
        [SerializeReference]
        public readonly IEffect data;

        public EffectModel(string key, IEffect data)
        {
            this.key = key;
            this.data = data;
        }
    }

    internal class EffectConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (!obj.TryGetValue(nameof(EffectModel.key), out var token))
                throw new ArgumentException($"{nameof(EffectModel.key)} not found");

            if (token.Type != JTokenType.String)
                throw new ArgumentException($"{nameof(EffectModel.key)} type not string");

            var key = token.Value<string>();

            if (!EffectLoader.TryGetEffectType(key, out var type))
                throw new KeyNotFoundException($"{nameof(EffectModel.key)} not register");

            var data = obj.GetValue(nameof(EffectModel.data)).ToObject(type) as IEffect;

            return new EffectModel(key, data);
        }

        public override bool CanConvert(Type objectType) => true;
    }
}