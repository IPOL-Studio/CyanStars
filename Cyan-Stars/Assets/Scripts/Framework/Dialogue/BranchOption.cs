

using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public class BranchOption
    {
        [JsonProperty("nextNode")]
        public int NextNodeID { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }
    }
}
