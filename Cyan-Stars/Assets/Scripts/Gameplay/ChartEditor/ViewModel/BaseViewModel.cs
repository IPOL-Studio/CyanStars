#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public abstract class BaseViewModel : IDisposable
    {
        protected readonly CommandManager CommandManager;
        protected readonly ChartEditorModel Model;

        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        protected BaseViewModel(ChartEditorModel model)
        {
            Model = model;

            var commandManager = GameRoot.GetDataModule<ChartModuleDataModule>().CommandManager;
            if (commandManager == null)
                throw new NullReferenceException("commandManager");
            CommandManager = commandManager;
        }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
