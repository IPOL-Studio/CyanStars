using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueData
    {
        [JsonProperty("nodes")]
        public List<NodeData> Nodes { get; set; }
    }
}
