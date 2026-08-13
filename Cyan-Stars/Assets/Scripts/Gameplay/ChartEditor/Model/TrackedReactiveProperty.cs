#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Command;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 带命令记录的响应式属性：值变化时自动生成撤销命令并压入 CommandStack
    /// </summary>
    public class TrackedReactiveProperty<T> : ReactiveProperty<T>
    {
        private readonly CommandStack CommandStack;

        public TrackedReactiveProperty(CommandStack commandStack, T initialValue)
            : base(initialValue)
        {
            CommandStack = commandStack;
        }

        public override T Value
        {
            set
            {
                var oldValue = CurrentValue;
                base.Value = value;

                // 基类构造期间字段尚未赋值，跳过记录；回放中或值未变化时也不生成命令
                if (CommandStack == null || CommandStack.IsReplaying || EqualityComparer<T>.Default.Equals(oldValue, value))
                    return;

                CommandStack.ExecuteCommand(
                    () => base.Value = value,
                    () => base.Value = oldValue
                );
            }
        }
    }
}
