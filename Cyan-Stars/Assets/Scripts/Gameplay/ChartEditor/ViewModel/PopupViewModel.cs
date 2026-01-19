#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class PopupViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<bool> CanvasVisibility => Model.PopupCanvasVisibility;
        public string TitleString => Model.PopupTitleString;
        public string DescribeString => Model.PopupDescribeString;
        public IReadOnlyDictionary<string, Action?> ButtonCallBackMap => Model.PopupButtonCallBackMap;
        public bool ShowCloseButton => Model.PopupShowCloseButton;

        public PopupViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
        }

        public void ClosePopup()
        {
            Model.ClosePopup();
        }
    }
}
