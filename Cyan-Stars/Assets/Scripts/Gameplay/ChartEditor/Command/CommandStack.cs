#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Command
{
    /// <summary>
    /// 命令管理器
    /// </summary>
    /// <remarks>使用 List 管理命令实例，以提供撤销重做功能</remarks>
    public class CommandStack : MonoBehaviour
    {
        private readonly List<ICommand> CommandHistory = new List<ICommand>();

        // 指向当前"最后一条已执行"的命令的索引
        // -1 表示没有任何命令被执行（初始状态或全部撤销）
        private int currentCommandIndex = -1;

        /// <summary>
        /// 执行新命令
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();

            // 如果当前索引不是在列表末尾，需要丢弃当前位置之后的所有旧历史
            if (currentCommandIndex < CommandHistory.Count - 1)
            {
                int removeStartIndex = currentCommandIndex + 1;
                int countToRemove = CommandHistory.Count - removeStartIndex;
                CommandHistory.RemoveRange(removeStartIndex, countToRemove);
            }

            CommandHistory.Add(command);
            currentCommandIndex++;

            // TODO: 可选添加最大历史记录限制，防止内存溢出
        }

        /// <summary>
        /// 撤销
        /// </summary>
        public void Undo()
        {
            // 如果索引为 -1，说明没有命令可以撤销
            if (currentCommandIndex < 0)
            {
                Debug.LogWarning("Nothing to undo");
                return;
            }

            CommandHistory[currentCommandIndex].Undo();
            currentCommandIndex--;
        }

        /// <summary>
        /// 重做
        /// </summary>
        public void Redo()
        {
            // 如果索引已经在列表末尾，说明没有命令可以重做
            if (currentCommandIndex >= CommandHistory.Count - 1)
            {
                Debug.LogWarning("Nothing to redo");
                return;
            }

            currentCommandIndex++;
            CommandHistory[currentCommandIndex].Execute();
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void Clear()
        {
            CommandHistory.Clear();
            currentCommandIndex = -1;
        }
    }
}
