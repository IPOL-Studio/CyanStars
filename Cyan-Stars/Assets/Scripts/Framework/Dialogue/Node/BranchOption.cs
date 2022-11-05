

using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public class BranchOption
    {
        [JsonProperty("nextNode")]
        public int NextNodeID { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{nameof(NextNodeID)}: {NextNodeID}, {nameof(Text)}: {Text}";
        }
    }
}
