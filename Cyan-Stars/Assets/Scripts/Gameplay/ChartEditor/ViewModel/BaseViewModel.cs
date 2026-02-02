#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public abstract class BaseViewModel : IDisposable
    {
        protected readonly CommandStack CommandStack;
        protected readonly ChartEditorModel Model;

        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        protected BaseViewModel(ChartEditorModel model)
        {
            Model = model;

            var commandStack = GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack;
            if (commandStack == null)
                throw new NullReferenceException("commandStack");
            CommandStack = commandStack;
        }

        public virtual void Dispose()
        {
            Disposables.Dispose();
        }
    }
}
