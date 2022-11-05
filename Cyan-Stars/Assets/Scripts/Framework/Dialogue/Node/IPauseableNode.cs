namespace CyanStars.Framework.Dialogue
{
    public interface IPauseableNode
    {
        /// <summary>
        /// Node是否可以在执行完成后，不用等待继续的信号，直接前进到下一个节点
        /// </summary>
        bool IsAutoContinue { get; }
    }
}
