using System.Collections.Generic;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueData
    {
        [JsonProperty("flowNodes")]
        public List<NodeData<BaseFlowNode>> FlowNodes { get; set; }

        [JsonProperty("initNodes")]
        public List<NodeData<BaseInitNode>> InitNodes { get; set; }
    }
}
