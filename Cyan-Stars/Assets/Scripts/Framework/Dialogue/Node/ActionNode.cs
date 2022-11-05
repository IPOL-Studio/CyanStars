using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Action")]
    public class ActionNode : BaseFlowNode, IPauseableNode
    {
        [JsonProperty("actions")]
        public List<BaseActionUnit> Actions { get; set; }

        [JsonProperty("autoContinue")]
        public bool IsAutoContinue { get; set; }

        public override async Task ExecuteAsync()
        {
            var tasks = new List<Task>(Actions.Count);
            foreach (var action in Actions)
            {
                tasks.Add(action.ExecuteAsync());
            }
            await Task.WhenAll(tasks);
        }
    }
}
