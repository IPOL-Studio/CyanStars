#nullable enable

using System;

namespace CyanStars.Gameplay.ChartEditor.Command
{
    public static class CommandStackExtend
    {
        public static void ExecuteCommand(this CommandStack commandStack, Action? executeAction, Action? undoAction)
        {
            commandStack.ExecuteCommand(new DelegateCommand(executeAction, undoAction));
        }
    }
}
