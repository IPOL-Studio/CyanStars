namespace CyanStars.Framework.Dialogue
{
    [DialogueNode("Entry")]
    public class EntryNode : BaseNode
    {
        public override void OnInit()
        {
            IsCompleted = true;
        }

        public override void OnUpdate(float deltaTime)
        {
        }
    }
}
