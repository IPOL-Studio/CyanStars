#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public abstract class BaseViewModel : IDisposable
    {
        protected readonly ChartEditorModel Model;
        protected readonly CommandManager CommandManager;


        protected BaseViewModel(ChartEditorModel model, CommandManager commandManager)
        {
            this.Model = model;
            this.CommandManager = commandManager;
        }

        // 静态 ViewModel 生命周期目前与 ChartEditorModel 一致，故不需要取消订阅来防止内存泄漏
        // 动态 ViewModel 需要在销毁时手动取消订阅以释放内存
        public virtual void Dispose()
        {
        }
    }
}
