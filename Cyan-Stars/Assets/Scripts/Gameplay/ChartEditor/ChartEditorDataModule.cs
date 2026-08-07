#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using R3;

namespace CyanStars.Gameplay.ChartEditor
{
    public class ChartEditorDataModule : BaseDataModule
    {
        public CommandStack CommandStack { get; private set; } = null!;

        /// <summary>
        /// 是否存在未保存数据（未进入制谱器时为 null，使用前请先判空）
        /// </summary>
        public ReadOnlyReactiveProperty<bool>? HasUnsavedChanges { get; private set; }


        public override void OnInit()
        {
        }

        public void OnEnterChartEditorProcedure(CommandStack targetCommandStack)
        {
            CommandStack = targetCommandStack;
            HasUnsavedChanges = targetCommandStack.HasUnsavedChanges;
        }

        public void OnExitChartEditorProcedure()
        {
            if (CommandStack == null)
                throw new Exception("未找到 CommandStack，未加载过或已经卸载？请检查业务逻辑。");

            CommandStack = null;
            HasUnsavedChanges = null;
        }
    }
}
