#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;

namespace CyanStars.GamePlay.ChartEditor
{
    public class ChartModuleDataModule : BaseDataModule
    {
        public CommandManager? CommandManager { get; private set; }


        public override void OnInit()
        {
        }

        public void OnEnterChartEditorProcedure(CommandManager targetCommandManager)
        {
            CommandManager = targetCommandManager;
        }

        public void OnExitChartEditorProcedure()
        {
            if (CommandManager == null)
                throw new Exception("未找到 CommandManager，未加载过或已经卸载？请检查业务逻辑。");

            CommandManager.Clear();
            CommandManager = null;
        }
    }
}
