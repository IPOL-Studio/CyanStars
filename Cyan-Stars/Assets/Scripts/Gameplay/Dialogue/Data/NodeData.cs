using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public class NodeData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("node")]
        public BaseNode Node { get; set; }
    }
}
