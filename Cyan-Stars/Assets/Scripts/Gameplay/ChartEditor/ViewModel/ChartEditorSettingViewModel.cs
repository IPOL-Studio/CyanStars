#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartEditorSettingViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<int> MusicVolume => Model.MusicVolume;
        public ReadOnlyReactiveProperty<int> NoteVolume => Model.NoteVolume;
        public ReadOnlyReactiveProperty<bool> IsCompactNoteButtonArea => Model.IsCompactNoteButtonArea;
        public ReadOnlyReactiveProperty<bool> IsMultiBpmItemMode => Model.IsMultiBpmItemMode;
        public ReadOnlyReactiveProperty<bool> IsMultiMusicItemMode => Model.IsMultiMusicItemMode;

        public ChartEditorSettingViewModel(ChartEditorModel model) : base(model)
        {
        }

        public void SetMusicVolume(int value) => Model.MusicVolume.Value = value;
        public void SetNoteVolume(int value) => Model.NoteVolume.Value = value;
        public void SetCompactNoteButtonArea(bool value) => Model.IsCompactNoteButtonArea.Value = value;
        public void SetMultiBpmItemMode(bool value) => Model.IsMultiBpmItemMode.Value = value;
        public void SetMultiMusicItemMode(bool value) => Model.IsMultiMusicItemMode.Value = value;
    }
}
