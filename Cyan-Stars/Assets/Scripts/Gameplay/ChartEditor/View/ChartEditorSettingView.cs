#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.RadioButton;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartEditorSettingView : BasePopupView<ChartEditorSettingViewModel>
    {
        [SerializeField]
        private RadioButtonItem compactNoteButtonAreaButton = null!;

        public override void Bind(ChartEditorSettingViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
            compactNoteButtonAreaButton.IsChecked = ViewModel.CompactNoteButtonArea.CurrentValue;
            compactNoteButtonAreaButton.OnValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool value) => ViewModel.SetCompactNoteButtonArea(value);

        protected override void OnDestroy()
        {
            compactNoteButtonAreaButton.OnValueChanged.RemoveListener(OnValueChanged);
            base.OnDestroy();
        }
    }
}
