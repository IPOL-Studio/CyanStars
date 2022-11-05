using System.Threading.Tasks;

namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("FlowEntry")]
    public class FlowEntryNode : BaseFlowNode, IEntryNode
    {
        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }
}
