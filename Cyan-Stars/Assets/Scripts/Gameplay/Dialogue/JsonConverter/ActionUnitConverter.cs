using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Gameplay.Dialogue
{
    public class ActionUnitConverter : JsonConverter<BaseActionUnit>
    {
        public static readonly ActionUnitConverter Converter = new ActionUnitConverter();

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, BaseActionUnit value, JsonSerializer serializer)
        {
        }

        public override BaseActionUnit ReadJson(JsonReader reader, Type objectType, BaseActionUnit existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeKey = jo.GetValue("type").Value<string>();

            var type = GameRoot.GetDataModule<DialogueMetadataModule>().GetActionUnitType(typeKey);

            return jo.GetValue("action").ToObject(type) as BaseActionUnit;
        }
    }
}
