using System.Threading.Tasks;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public static class DialogueDataHelper
    {
        private static readonly NodeDataJsonConverter<BaseFlowNode> FlowNodeDataConverter = new NodeDataJsonConverter<BaseFlowNode>();
        private static readonly NodeDataJsonConverter<BaseInitNode> InitNodeDataConverter = new NodeDataJsonConverter<BaseInitNode>();

        public static async Task<DialogueData> Deserialize(string json)
        {
            return await Task.Run(() =>
                JsonConvert.DeserializeObject<DialogueData>(json, FlowNodeDataConverter, InitNodeDataConverter,
                    ActionUnitConverter.Converter));
        }
    }
}
