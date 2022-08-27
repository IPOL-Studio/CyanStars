namespace CyanStars.Framework.Dialogue
{
    /// <summary>
    /// Wait mark for ActionUnit
    /// <para>ActionNode check its holding of ActionUnit</para>
    /// <para>if any ActionUnit impl this interface</para>
    /// <para>the ActionNode  <see cref="BaseFlowNode.GotoNextType"/> will return <see cref="GotoNextNodeActionType.Wait"/></para>
    /// </summary>
    public interface IWaitActionUnit
    {

    }
}
