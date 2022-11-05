using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Branch")]
    public class BranchNode : BaseFlowNode
    {
        [JsonProperty("options")]
        public List<BranchOption> Options { get; set; }

        public override async Task ExecuteAsync()
        {
            var chooser = GameRoot.Dialogue.GetService<IBranchChooser>();
            await chooser.ShowOptionsAsync(Options);
            var option = await chooser.GetSelectOptionAsync();
            NextNodeID = option.NextNodeID;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Option count: {Options.Count}";
        }
    }
}
