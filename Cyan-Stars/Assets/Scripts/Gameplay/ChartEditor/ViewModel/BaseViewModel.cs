#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public abstract class BaseViewModel
    {
        protected readonly ChartEditorModel Model;
        protected readonly CommandManager CommandManager;


        protected BaseViewModel(ChartEditorModel model, CommandManager commandManager)
        {
            this.Model = model;
            this.CommandManager = commandManager;
        }

        // BaseViewModel 生命周期目前与 ChartEditorModel 一致，故不需要取消订阅来防止内存泄漏。
    }
}
