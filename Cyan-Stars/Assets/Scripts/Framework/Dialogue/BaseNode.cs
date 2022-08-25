using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseNode
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("nextNode")]
        public int NextNodeID { get; set; }

        [JsonIgnore]
        public bool IsCompleted { get; protected set; }
    }
}
