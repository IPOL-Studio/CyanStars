#nullable enable

using System.Diagnostics.Contracts;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.RadioButton;
using UnityEngine;
using R3;
using TMPro;
using UnityEngine.Audio;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartEditorSettingView : BasePopupView<ChartEditorSettingViewModel>
    {
        [SerializeField]
        private AudioMixer audioMixer = null!;

        [SerializeField]
        private TMP_InputField musicVolumeField = null!;

        [SerializeField]
        private TMP_InputField noteVolumeField = null!;

        [SerializeField]
        private RadioButtonItem isMultiBpmItemButton = null!;

        [SerializeField]
        private RadioButtonItem isMultiMusicItemButton = null!;

        [SerializeField]
        private RadioButtonItem isCompactNoteButtonAreaButton = null!;

        [SerializeField]
        private RadioButtonItem isShowingAudioWave = null!;


        // AudioMixer 中的变量名
        private const string MusicVolumeName = "MusicVolume";
        private const string NoteVolumeName = "NoteVolume";


        public override void Bind(ChartEditorSettingViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.MusicVolume
                .Subscribe(value =>
                {
                    musicVolumeField.text = value.ToString();
                    audioMixer.SetFloat(MusicVolumeName, CalculateVolumeDb(value));
                })
                .AddTo(this);
            ViewModel.NoteVolume
                .Subscribe(value =>
                {
                    noteVolumeField.text = value.ToString();
                    audioMixer.SetFloat(NoteVolumeName, CalculateVolumeDb(value));
                })
                .AddTo(this);

            musicVolumeField.OnEndEditAsObservable()
                .Subscribe(value =>
                {
                    if (!int.TryParse(value, out var musicVolumeInt) || musicVolumeInt < 0 || musicVolumeInt > 100)
                    {
                        musicVolumeField.text = ViewModel.MusicVolume.CurrentValue.ToString(); // 这里就简单一点直接获取，不从 Model 强制刷新了
                        return;
                    }

                    ViewModel.SetMusicVolume(musicVolumeInt);
                })
                .AddTo(this);
            noteVolumeField.onEndEdit.AsObservable()
                .Subscribe(value =>
                {
                    if (!int.TryParse(value, out var noteVolumeInt) || noteVolumeInt < 0 || noteVolumeInt > 100)
                    {
                        noteVolumeField.text = ViewModel.NoteVolume.CurrentValue.ToString();
                        return;
                    }

                    ViewModel.SetNoteVolume(noteVolumeInt);
                })
                .AddTo(this);

            isMultiBpmItemButton.IsChecked = ViewModel.IsMultiBpmItemMode.CurrentValue;
            isMultiMusicItemButton.IsChecked = ViewModel.IsMultiMusicItemMode.CurrentValue;
            isCompactNoteButtonAreaButton.IsChecked = ViewModel.IsCompactNoteButtonArea.CurrentValue;
            isShowingAudioWave.IsChecked = ViewModel.IsShowingAudioWave.CurrentValue;

            isMultiBpmItemButton.OnValueChanged.AddListener(ViewModel.SetMultiBpmItemMode);
            isMultiMusicItemButton.OnValueChanged.AddListener(ViewModel.SetMultiMusicItemMode);
            isCompactNoteButtonAreaButton.OnValueChanged.AddListener(ViewModel.SetCompactNoteButtonArea);
            isShowingAudioWave.OnValueChanged.AddListener(ViewModel.SetShowingAudioWave);
        }

        protected override void OnDestroy()
        {
            isMultiBpmItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiBpmItemMode);
            isMultiMusicItemButton.OnValueChanged.RemoveListener(ViewModel.SetMultiMusicItemMode);
            isCompactNoteButtonAreaButton.OnValueChanged.RemoveListener(ViewModel.SetCompactNoteButtonArea);
            isShowingAudioWave.OnValueChanged.RemoveListener(ViewModel.SetShowingAudioWave);
            base.OnDestroy();
        }

        [Pure]
        private static float CalculateVolumeDb(int volumeInt)
        {
            return volumeInt switch
            {
                <= 0 => -80,
                >= 100 => 0,
                _ => Mathf.Log10(volumeInt / 100f) * 20f
            };
        }
    }
}
