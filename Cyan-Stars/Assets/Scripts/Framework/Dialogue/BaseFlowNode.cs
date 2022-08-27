namespace CyanStars.Framework.Dialogue
{
    public abstract class BaseFlowNode : BaseNode
    {
        /// <summary>
        /// go to next node action type after complete
        /// </summary>
        public virtual GotoNextNodeActionType GotoNextType => GotoNextNodeActionType.Direct;

        public abstract void OnInit();
        public abstract void OnUpdate(float deltaTime);
        public abstract void OnComplete();
    }
}
