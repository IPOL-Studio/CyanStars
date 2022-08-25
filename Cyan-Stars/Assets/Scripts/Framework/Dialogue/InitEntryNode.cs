using System.ComponentModel;
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

        public override void OnInit()
        {
            //TODO: 实现InitEntry
        }
    }
}
