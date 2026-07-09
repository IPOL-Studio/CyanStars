#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.RadioButton;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartEditorSettingView : BasePopupView<ChartEditorSettingViewModel>
    {
        [SerializeField]
        private RadioButtonItem isCompactNoteButtonAreaButton = null!;

        [SerializeField]
        private RadioButtonItem isMultiBpmItemButton = null!;

        [SerializeField]
        private RadioButtonItem isMultiMusicItemButton = null!;


        public override void Bind(ChartEditorSettingViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            isCompactNoteButtonAreaButton.IsChecked = ViewModel.IsCompactNoteButtonArea.CurrentValue;
            isMultiBpmItemButton.IsChecked = ViewModel.IsMultiBpmItemMode.CurrentValue;
            isMultiMusicItemButton.IsChecked = ViewModel.IsMultiMusicItemMode.CurrentValue;
            isCompactNoteButtonAreaButton.OnValueChanged.AddListener(ViewModel.SetCompactNoteButtonArea);
            isMultiBpmItemButton.OnValueChanged.AddListener(ViewModel.SetMultiBpmItemMode);
            isMultiMusicItemButton.OnValueChanged.AddListener(ViewModel.SetMultiMusicItemMode);
        }

        protected override void OnDestroy()
        {
            isCompactNoteButtonAreaButton.OnValueChanged.RemoveListener(ViewModel.SetCompactNoteButtonArea);
            isMultiBpmItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiBpmItemMode);
            isMultiMusicItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiMusicItemMode);
            base.OnDestroy();
        }
    }
}
