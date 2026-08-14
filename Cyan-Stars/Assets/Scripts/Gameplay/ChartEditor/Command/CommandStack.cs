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
        private class CommandEntry
        {
            public readonly ICommand Command;
            public readonly bool AffectsSavedData;

            public CommandEntry(ICommand command, bool affectsSavedData)
            {
                Command = command;
                AffectsSavedData = affectsSavedData;
            }
        }

        // 预分配容量到历史上限，避免 List 默认从 4 items 开始反复扩容复制
        private readonly List<CommandEntry> CommandHistory = new List<CommandEntry>(MaxHistoryCount);

        // 历史记录上限，超出后从最旧开始丢弃，防止内存溢出
        private const int MaxHistoryCount = 100;

        // 指向当前"最后一条已执行"的命令的索引
        // -1 表示没有任何命令被执行（初始状态或全部撤销）
        private int currentCommandIndex = -1;

        // 保存边界：最近一次保存时数据状态对应的数据命令条目。
        private CommandEntry? savedDataEntry = null;

        // 当前数据状态对应的最近一条 affects 命令条目（当前索引向下最近一条 affects 命令），无则为 null。
        private CommandEntry? lastAffectsEntry = null;

        // 旁路修改（不经过命令栈的数据写入）置脏标记，MarkSaved 时清除
        private bool forcedDirty = false;

        private readonly ReactiveProperty<bool> hasUnsavedChanges = new ReactiveProperty<bool>(false);

        /// <summary>
        /// 是否存在未保存数据（订阅时立即推送当前值）
        /// </summary>
        public ReadOnlyReactiveProperty<bool> HasUnsavedChanges => hasUnsavedChanges;

        /// <summary>
        /// 每次进入制谱器会话时初始化
        /// </summary>
        /// <param name="initialHasUnsavedChanges">会话初始是否存在未保存数据（新建谱面为 true，加载已有谱面为 false）</param>
        public void Init(bool initialHasUnsavedChanges)
        {
            CommandHistory.Clear();
            currentCommandIndex = -1;
            savedDataEntry = null;
            lastAffectsEntry = null;
            forcedDirty = initialHasUnsavedChanges;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 执行新命令
        /// </summary>
        /// <param name="command">要执行的命令</param>
        /// <param name="affectsSavedData">该命令是否修改持久化数据。纯视图/选中状态的命令传 false，不参与脏判定</param>
        public void ExecuteCommand(ICommand command, bool affectsSavedData = true)
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

            var entry = new CommandEntry(command, affectsSavedData);
            CommandHistory.Add(entry);
            currentCommandIndex++;

            if (affectsSavedData)
                lastAffectsEntry = entry;

            // 超出历史上限时丢弃最旧的命令。若保存边界条目被丢弃，数据不可能再与磁盘一致，引用比较会保持脏状态直到下次保存
            if (CommandHistory.Count > MaxHistoryCount)
            {
                // 被丢弃的最旧条目若正是当前 lastAffectsEntry，说明已无法回退到该状态，置空以保持脏状态
                if (lastAffectsEntry == CommandHistory[0])
                    lastAffectsEntry = null;

                CommandHistory.RemoveAt(0);
                currentCommandIndex--;
            }

            UpdateUnsavedState();
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

            var entryToUndo = CommandHistory[currentCommandIndex];
            entryToUndo.Command.Undo();

            // 撤销的是 affects 命令时，lastAffectsEntry 需要回退到上一条 affects 命令
            if (entryToUndo.AffectsSavedData)
                lastAffectsEntry = FindLastAffectsEntry(currentCommandIndex - 1);

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
            var entryToRedo = CommandHistory[currentCommandIndex];
            entryToRedo.Command.Execute();

            if (entryToRedo.AffectsSavedData)
                lastAffectsEntry = entryToRedo;

            UpdateUnsavedState();
        }

        /// <summary>
        /// 标记当前数据为已保存（保存成功后调用）
        /// </summary>
        public void MarkSaved()
        {
            forcedDirty = false;
            savedDataEntry = lastAffectsEntry;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 强制标记为有未保存数据（不经过命令栈的旁路数据修改使用）
        /// </summary>
        public void MarkDirty()
        {
            forcedDirty = true;
            UpdateUnsavedState();
        }

        /// <summary>
        /// 从 fromIndex 向前查找最近一条 affects 命令，无则返回 null
        /// </summary>
        private CommandEntry? FindLastAffectsEntry(int fromIndex)
        {
            for (int i = fromIndex; i >= 0; i--)
            {
                if (CommandHistory[i].AffectsSavedData)
                    return CommandHistory[i];
            }

            return null;
        }

        /// <summary>
        /// 根据当前数据命令条目与保存边界条目是否相同来重算未保存状态
        /// </summary>
        private void UpdateUnsavedState()
        {
            hasUnsavedChanges.Value = forcedDirty || savedDataEntry != lastAffectsEntry;
        }
    }
}
