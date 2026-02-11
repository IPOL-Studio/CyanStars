#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;

namespace CyanStars.Gameplay.ChartEditor
{
    public class ChartEditorDataModule : BaseDataModule
    {
        public CommandStack CommandStack { get; private set; } = null!;


        public override void OnInit()
        {
        }

        public void OnEnterChartEditorProcedure(CommandStack targetCommandStack)
        {
            CommandStack = targetCommandStack;
        }

        public void OnExitChartEditorProcedure()
        {
            if (CommandStack == null)
                throw new Exception("未找到 CommandStack，未加载过或已经卸载？请检查业务逻辑。");

            CommandStack = null;
        }
    }
}
