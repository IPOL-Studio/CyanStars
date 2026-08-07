#nullable enable

using System.Collections.Generic;
using R3;
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

        // 干净边界：最近一次成功保存时的命令索引，与 currentCommandIndex 相等即代表无未保存数据
        // 若边界落在被丢弃的历史中（保存后撤销再执行新命令），数据不可能再与磁盘一致，
        // 此时置为 int.MinValue 使其不可达，直到下一次保存
        private int cleanBoundaryIndex = -1;

        private readonly ReactiveProperty<bool> hasUnsavedChanges = new ReactiveProperty<bool>(false);

        /// <summary>
        /// 是否存在未保存数据（订阅时立即推送当前值）
        /// </summary>
        public ReadOnlyReactiveProperty<bool> HasUnsavedChanges => hasUnsavedChanges;

        /// <summary>
        /// 执行新命令
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            // TODO: 用事件驱动以替换当前的命令调用，以避免 View/MonoBehaviour 的内存泄漏
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

            // 若干净边界落在被丢弃的历史中，数据不可能再与磁盘一致
            if (cleanBoundaryIndex > currentCommandIndex)
                cleanBoundaryIndex = int.MinValue;

            UpdateUnsavedState();

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
            UpdateUnsavedState();
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
            UpdateUnsavedState();
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void Clear()
        {
            CommandHistory.Clear();
            currentCommandIndex = -1;
            cleanBoundaryIndex = -1;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 标记当前数据为已保存（保存成功后调用）
        /// </summary>
        public void MarkSaved()
        {
            cleanBoundaryIndex = currentCommandIndex;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 强制标记为有未保存数据（新建谱面等不经过命令栈的数据修改使用）
        /// </summary>
        public void MarkDirty()
        {
            cleanBoundaryIndex = int.MinValue;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 根据命令位置与干净边界是否相等来重算未保存状态
        /// </summary>
        private void UpdateUnsavedState()
        {
            hasUnsavedChanges.Value = currentCommandIndex != cleanBoundaryIndex;
        }
    }
}
