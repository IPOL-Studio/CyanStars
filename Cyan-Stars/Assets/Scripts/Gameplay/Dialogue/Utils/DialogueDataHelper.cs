using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public static class DialogueDataHelper
    {
        private static readonly JsonSerializerSettings DeserializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Populate,
            Converters = new List<JsonConverter>
            {
                new NodeDataJsonConverter<BaseFlowNode>(),
                new NodeDataJsonConverter<BaseInitNode>(),
                ActionUnitConverter.Converter
            }
        };

        public static async Task<DialogueData> Deserialize(string json)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<DialogueData>(json, DeserializerSettings));
        }
    }
}
