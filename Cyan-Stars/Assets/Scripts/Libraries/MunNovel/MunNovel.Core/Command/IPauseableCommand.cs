namespace MunNovel.Command
{
    public interface IPauseableCommand
    {
        bool IsPause { get; }
        double PauseTime { get; }
    }
}
