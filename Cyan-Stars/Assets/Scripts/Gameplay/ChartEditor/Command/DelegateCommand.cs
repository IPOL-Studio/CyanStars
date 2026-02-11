#nullable enable

using System;

namespace CyanStars.Gameplay.ChartEditor.Command
{
    /// <summary>
    /// 通用命令类，无需再为每个可撤销操作实例化 ICommand 类，而是直接用 Lambda 表达式传入撤销重做时的函数
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action? ExecuteAction;
        private readonly Action? UndoAction;


        public DelegateCommand(Action? execute, Action? undo)
        {
            ExecuteAction = execute;
            UndoAction = undo;
        }

        public void Execute()
        {
            ExecuteAction?.Invoke();
        }

        public void Undo()
        {
            UndoAction?.Invoke();
        }
    }
}
