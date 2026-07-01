#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartEditorSettingViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<bool> CompactNoteButtonArea => Model.CompactNoteButtonArea;

        public ChartEditorSettingViewModel(ChartEditorModel model) : base(model)
        {
        }

        public void SetCompactNoteButtonArea(bool value) => Model.CompactNoteButtonArea.Value = value;
    }
}
