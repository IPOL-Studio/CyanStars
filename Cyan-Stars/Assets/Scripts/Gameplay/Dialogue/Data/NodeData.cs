using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public class NodeData<T> where T : BaseNode
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("node")]
        public T Node { get; set; }
    }
}
