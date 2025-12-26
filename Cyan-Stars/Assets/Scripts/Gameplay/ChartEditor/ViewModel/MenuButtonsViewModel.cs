#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class MenuButtonsViewModel : BaseViewModel
    {
        private readonly ReactiveProperty<bool> functionCanvasVisibility;
        public readonly ReadOnlyReactiveProperty<bool> FunctionCanvasVisibility;


        public MenuButtonsViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            functionCanvasVisibility = new ReactiveProperty<bool>(false);
            FunctionCanvasVisibility = functionCanvasVisibility.ToReadOnlyReactiveProperty();
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
            ));
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
                // 如果 EffectTracksCanvas 还没做，暂时先返回 null 或者抛出更友好的错误
                case CanvasType.EffectTracksCanvas:
                    throw new NotImplementedException("EffectTracksCanvas is not implemented yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(canvasType), canvasType, null);
            }
        }
    }
}
