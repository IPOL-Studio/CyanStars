using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Dialogue
{
    public class RichTextData
    {
        [JsonProperty("attrs")]
        public List<string> Attributes { get; set; }

        [JsonProperty("attrValues")]
        public Dictionary<string, string> AttributeValues { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
