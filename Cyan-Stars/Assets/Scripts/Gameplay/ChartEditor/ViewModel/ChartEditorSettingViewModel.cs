#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartEditorSettingViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<bool> CompactNoteButtonArea => Model.CompactNoteButtonArea;
        public ReadOnlyReactiveProperty<bool> MultiBpmItemMode => Model.MultiBpmItemMode;
        public ReadOnlyReactiveProperty<bool> MultiMusicItemMode => Model.MultiMusicItemMode;

        public ChartEditorSettingViewModel(ChartEditorModel model) : base(model)
        {
        }

        public void SetCompactNoteButtonArea(bool value) => Model.CompactNoteButtonArea.Value = value;
        public void SetMultiBpmItemMode(bool value) => Model.MultiBpmItemMode.Value = value;
        public void SetMultiMusicItemMode(bool value) => Model.MultiMusicItemMode.Value = value;
    }
}
