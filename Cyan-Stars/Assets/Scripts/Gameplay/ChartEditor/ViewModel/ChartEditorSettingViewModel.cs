#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartEditorSettingViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<bool> IsCompactNoteButtonArea => Model.IsCompactNoteButtonArea;
        public ReadOnlyReactiveProperty<bool> IsMultiBpmItemMode => Model.IsMultiBpmItemMode;
        public ReadOnlyReactiveProperty<bool> IsMultiMusicItemMode => Model.IsMultiMusicItemMode;

        public ChartEditorSettingViewModel(ChartEditorModel model) : base(model)
        {
        }

        public void SetCompactNoteButtonArea(bool value) => Model.IsCompactNoteButtonArea.Value = value;
        public void SetMultiBpmItemMode(bool value) => Model.IsMultiBpmItemMode.Value = value;
        public void SetMultiMusicItemMode(bool value) => Model.IsMultiMusicItemMode.Value = value;
    }
}
