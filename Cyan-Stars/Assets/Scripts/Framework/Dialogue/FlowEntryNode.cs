namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("FlowEntry")]
    public class FlowEntryNode : BaseFlowNode, IEntryNode
    {
        public override void OnInit()
        {
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnComplete()
        {

        }
    }
}
