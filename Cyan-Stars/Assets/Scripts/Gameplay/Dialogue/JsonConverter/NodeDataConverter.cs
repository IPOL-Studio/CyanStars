using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Gameplay.Dialogue
{
    public class NodeDataConverter : JsonConverter<NodeData>
    {
        public static readonly NodeDataConverter Converter = new NodeDataConverter();

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, NodeData value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override NodeData ReadJson(JsonReader reader, Type objectType, NodeData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeStr = jo.GetValue("type").Value<string>();
            var type = GameRoot.GetDataModule<DialogueMetadataModule>().GetNodeType(typeStr);
            var node = serializer.Deserialize(jo.GetValue("node").CreateReader(), type) as BaseNode;
            return new NodeData
            {
                Type = typeStr,
                Node = node
            };
        }
    }
}
