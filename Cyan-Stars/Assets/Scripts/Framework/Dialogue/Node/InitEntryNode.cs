using System.ComponentModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("InitEntry")]
    public class InitEntryNode : BaseInitNode, IEntryNode
    {
        [JsonProperty("isShowDialogueBox")]
        [DefaultValue(true)]
        public bool IsShowDialogueBox { get; set; } = true;

        [JsonProperty("backgroundFilePath")]
        public string BackgroundFilePath { get; set; }

        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }
}
