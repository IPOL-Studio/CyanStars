namespace CyanStars.Gameplay.ChartEditor.Command
{
    /// <summary>
    /// 用于撤销重做系统的接口
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
