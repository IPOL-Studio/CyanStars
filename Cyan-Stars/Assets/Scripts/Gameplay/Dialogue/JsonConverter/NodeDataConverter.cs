using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Gameplay.Dialogue
{
    public class NodeDataJsonConverter<T> : JsonConverter<NodeData<T>> where T : BaseNode
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, NodeData<T> value, JsonSerializer serializer)
        {
        }

        public override NodeData<T> ReadJson(JsonReader reader, Type objectType, NodeData<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeStr = jo.GetValue("type").Value<string>();
            var type = GameRoot.GetDataModule<DialogueMetadataModule>().GetNodeType(typeStr);
            var node = jo.GetValue("node").ToObject(type, serializer) as T;
            return new NodeData<T>
            {
                Type = typeStr,
                Node = node
            };
        }
    }
}
