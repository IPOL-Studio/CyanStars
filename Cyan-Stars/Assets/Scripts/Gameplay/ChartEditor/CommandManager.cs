using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 用于撤销重做系统的接口
    /// </summary>
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CommandManager : MonoBehaviour
    {
        private readonly Stack<ICommand> UndoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> RedoStack = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            UndoStack.Push(command);
            RedoStack.Clear();
        }

        public void Undo()
        {
            if (UndoStack.Count > 0)
            {
                var command = UndoStack.Pop();
                command.Undo();
                RedoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (RedoStack.Count > 0)
            {
                var command = RedoStack.Pop();
                command.Execute();
                UndoStack.Push(command);
            }
        }
    }
}
