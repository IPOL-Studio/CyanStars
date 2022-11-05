using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseNode
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("nextNode")]
        [DefaultValue(-1)]
        public int NextNodeID { get; set; }

        public abstract Task ExecuteAsync();

        public override string ToString()
        {
            return $"ID: {ID}, NextNodeID: {NextNodeID}";
        }
    }
}
