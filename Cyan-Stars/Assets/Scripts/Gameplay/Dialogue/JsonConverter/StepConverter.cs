using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Gameplay.Dialogue
{
    public class StepConverter : JsonConverter<BaseStep>
    {
        public static readonly StepConverter Converter = new StepConverter();

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, BaseStep value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override BaseStep ReadJson(JsonReader reader, Type objectType, BaseStep existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeKey = jo.GetValue("type").Value<string>();

            var type = GameRoot.GetDataModule<DialogueMetadataModule>().GetStepType(typeKey);

            return jo.GetValue("step").ToObject(type) as BaseStep;
        }
    }
}
