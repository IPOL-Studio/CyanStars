namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseFlowNode : BaseNode
    {
        public abstract void OnInit();
        public abstract void OnUpdate(float deltaTime);
        public abstract void OnComplete();
    }
}
