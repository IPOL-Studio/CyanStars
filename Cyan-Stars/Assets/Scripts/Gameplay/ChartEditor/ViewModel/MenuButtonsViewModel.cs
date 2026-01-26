#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MenuButtonsViewModel : BaseViewModel
    {
        private readonly ReactiveProperty<bool> functionCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReadOnlyReactiveProperty<bool> FunctionCanvasVisibility;

        public readonly ReadOnlyReactiveProperty<bool> IsSimplificationMode;


        public MenuButtonsViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            FunctionCanvasVisibility = functionCanvasVisibility.ToReadOnlyReactiveProperty().AddTo(Disposables);
            IsSimplificationMode = Model.IsSimplificationMode.ToReadOnlyReactiveProperty().AddTo(Disposables);
        }

        public void SetFunctionCanvasVisibility(bool newValue)
        {
            if (functionCanvasVisibility.Value == newValue)
                return;

            functionCanvasVisibility.Value = newValue;
        }

        public void OpenCanvas(CanvasType canvasType)
        {
            var property = GetCanvasVisibilityProperty(canvasType);

            if (property.Value)
                return;


            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                    {
                        property.Value = true;
                    },
                    () =>
                    {
                        property.Value = false;
                    }
                )
            );
        }

        public void SetSimplificationMode(bool newValue)
        {
            if (newValue == Model.IsSimplificationMode.Value)
                return;

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.IsSimplificationMode.Value = newValue;
                    },
                    () =>
                    {
                        Model.IsSimplificationMode.Value = !newValue;
                    }
                )
            );
        }

        public void SaveFileToDesk()
        {
            ChartEditorFileManager.SaveChartToDesk(
                Model.WorkspacePath,
                Model.ChartMetaDataIndex,
                Model.ChartPackData.CurrentValue,
                Model.ChartData.CurrentValue
            );
        }


        private ReactiveProperty<bool> GetCanvasVisibilityProperty(CanvasType canvasType)
        {
            switch (canvasType)
            {
                case CanvasType.ChartDataCanvas:
                    return Model.ChartDataCanvasVisibility;
                case CanvasType.ChartPackDataCanvas:
                    return Model.ChartPackDataCanvasVisibility;
                case CanvasType.MusicVersionCanvas:
                    return Model.MusicVersionCanvasVisibility;
                case CanvasType.BpmGroupCanvas:
                    return Model.BpmGroupCanvasVisibility;
                case CanvasType.SpeedTemplateCanvas:
                    return Model.SpeedTemplateCanvasVisibility;
                case CanvasType.EffectTracksCanvas:
                    throw new NotImplementedException("EffectTracksCanvas is not implemented yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(canvasType), canvasType, null);
            }
        }
    }
}
