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

        [SerializeField]
        private RadioButtonItem multiBpmItemButton = null!;

        [SerializeField]
        private RadioButtonItem multiMusicItemButton = null!;


        public override void Bind(ChartEditorSettingViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
            compactNoteButtonAreaButton.IsChecked = ViewModel.CompactNoteButtonArea.CurrentValue;
            multiBpmItemButton.IsChecked = ViewModel.MultiBpmItemMode.CurrentValue;
            multiMusicItemButton.IsChecked = ViewModel.MultiMusicItemMode.CurrentValue;
            compactNoteButtonAreaButton.OnValueChanged.AddListener(ViewModel.SetCompactNoteButtonArea);
            multiBpmItemButton.OnValueChanged.AddListener(ViewModel.SetMultiBpmItemMode);
            multiMusicItemButton.OnValueChanged.AddListener(ViewModel.SetMultiMusicItemMode);
        }

        protected override void OnDestroy()
        {
            compactNoteButtonAreaButton.OnValueChanged.RemoveListener(ViewModel.SetCompactNoteButtonArea);
            multiBpmItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiBpmItemMode);
            multiMusicItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiMusicItemMode);
            base.OnDestroy();
        }
    }
}
