#nullable enable

using System;

namespace CyanStars.Gameplay.ChartEditor.Command
{
    /// <summary>
    /// 命令拓展方法，无需再为每个可撤销操作实例化 ICommand 类，而是直接用 Lambda 表达式传入撤销重做时的函数
    /// </summary>
    public static class CommandStackExtend
    {
        public static void ExecuteCommand(this CommandStack commandStack, Action? executeAction, Action? undoAction)
        {
            commandStack.ExecuteCommand(new DelegateCommand(executeAction, undoAction));
        }

        private class DelegateCommand : ICommand
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
}
