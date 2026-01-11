#nullable enable

using System;

namespace CyanStars.Gameplay.ChartEditor.Command
{
    /// <summary>
    /// 通用命令类，用 Lambda 表达式简化撤销重做命令
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action? ExecuteAction;
        private readonly Action? UndoAction;


        public DelegateCommand(Action execute, Action undo)
        {
            ExecuteAction = execute;
            UndoAction = undo;
        }

        public void Execute() => ExecuteAction?.Invoke();
        public void Undo() => UndoAction?.Invoke();
    }
}
