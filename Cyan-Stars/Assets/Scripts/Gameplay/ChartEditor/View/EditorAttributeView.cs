#nullable enable

using System;
using System.Globalization;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.SelectableUI;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditorAttributeView : BaseView<EditorAttributeViewModel>
    {
        [Header("编辑器属性")]
        [SerializeField]
        private AudioMixer audioMixer = null!;

        [SerializeField]
        private AudioSource musicAudioSource = null!;

        [SerializeField]
        private Canvas editorAttributeCanvas = null!;

        [SerializeField]
        private TMP_InputField posAccuracyField = null!;

        [SerializeField]
        private Toggle posMagnetToggle = null!;

        [SerializeField]
        private TMP_InputField beatAccuracyField = null!;

        [SerializeField]
        private Button minusBeatAccuracyButton = null!;

        [SerializeField]
        private Button addBeatAccuracyButton = null!;

        [SerializeField]
        private TMP_InputField beatZoomField = null!;

        [SerializeField]
        private Button zoomOutButton = null!;

        [SerializeField]
        private Button zoomInButton = null!;

        [SerializeField]
        private TMP_InputField playbackSpeedField = null!;

        [SerializeField]
        private Button minusPlaybackSpeedButton = null!;

        [SerializeField]
        private Button addPlaybackSpeedButton = null!;


        [Header("进度条")]
        [SerializeField]
        private Slider slider = null!;

        [SerializeField]
        private TMP_Text musicTimeText = null!;

        [SerializeField]
        private Button playPauseButton = null!;

        [SerializeField]
        private Image playPauseImage = null!;

        [SerializeField]
        private Sprite playSprite = null!;

        [SerializeField]
        private Sprite pauseSprite = null!;


        // AudioMixer 中的变量名
        private const string MusicPitchName = "Music_PitchShifter_Pitch";

        private ReadOnlyReactiveProperty<bool> frameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> isTimelineReadyToPlay = null!; // 已加载音乐 && bpm 组有至少 1 个 item && music 组有至少 1 个 item
        private ReadOnlyReactiveProperty<int?> firstMusicOffset = null!; // 缓存的首个音乐的可观察 offset
        private bool isTimelineTimeChangeBySlider = false; // 防止拖拽/滚动进度条更新 time 后再做一次无意义的 scrollRect 位置更新


        public override void Bind(EditorAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.CanMinusBeatAccuracy
                .Subscribe(able =>
                {
                    if (minusBeatAccuracyButton.gameObject.TryGetComponent(out SelectableStateObserver observer))
                        observer.SetInteractable(able);
                    else
                        minusBeatAccuracyButton.interactable = able;
                })
                .AddTo(this);
            ViewModel.CanMinusBeatZoom
                .Subscribe(able =>
                {
                    if (zoomOutButton.gameObject.TryGetComponent(out SelectableStateObserver observer))
                        observer.SetInteractable(able);
                    else
                        zoomOutButton.interactable = able;
                })
                .AddTo(this);
            ViewModel.CanMinusPlaybackSpeed
                .Subscribe(able =>
                {
                    if (minusPlaybackSpeedButton.gameObject.TryGetComponent(out SelectableStateObserver observer))
                        observer.SetInteractable(able);
                    else
                        minusPlaybackSpeedButton.interactable = able;
                })
                .AddTo(this);

            frameVisibility =
                ViewModel.SelectedNoteData
                    .Select(note => note == null)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this);

            frameVisibility
                .Subscribe(value => editorAttributeCanvas.enabled = value)
                .AddTo(this);
            ViewModel.PosAccuracyString
                .Subscribe(value => posAccuracyField.text = value)
                .AddTo(this);
            ViewModel.PosMagnetState
                .Subscribe(value => posMagnetToggle.isOn = value)
                .AddTo(this);
            ViewModel.BeatAccuracyString
                .Subscribe(value => beatAccuracyField.text = value)
                .AddTo(this);
            ViewModel.BeatZoomString
                .Subscribe(value => beatZoomField.text = value)
                .AddTo(this);
            ViewModel.PlaybackSpeed
                .Subscribe(value =>
                {
                    playbackSpeedField.text = value.ToString(CultureInfo.InvariantCulture);
                    musicAudioSource.pitch = (float)value;
                    audioMixer.SetFloat(MusicPitchName, 1 / (float)value);
                })
                .AddTo(this);

            isTimelineReadyToPlay = Observable
                .CombineLatest(
                    ViewModel.AudioClipHandler,
                    ViewModel.BpmGroup.ObserveCountChanged(notifyCurrentCount: true),
                    ViewModel.MusicVersions.ObserveCountChanged(notifyCurrentCount: true),
                    (audioClipHandler, bpmGroupCount, musicVersionCount) =>
                        audioClipHandler?.Asset != null && bpmGroupCount >= 1 && musicVersionCount >= 1
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(this);
            var firstMusicVersionObservable = ViewModel.MusicVersions
                .ObserveChanged()
                .Select(_ => ViewModel.MusicVersions.Count > 0 ? ViewModel.MusicVersions[0] : null)
                .Prepend(ViewModel.MusicVersions.Count > 0 ? ViewModel.MusicVersions[0] : null);
            firstMusicOffset = firstMusicVersionObservable
                .Select(version => version == null
                    ? Observable.Return<int?>(null)
                    : version.Offset.Select(offset => (int?)offset)
                )
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(this);

            isTimelineReadyToPlay
                .Subscribe(interactable =>
                {
                    slider.interactable = interactable;
                    playPauseButton.interactable = interactable;
                })
                .AddTo(this);
            Observable.CombineLatest(
                    ViewModel.CurrentTimelineTimeMs,
                    firstMusicOffset,
                    ViewModel.AudioClipHandler,
                    (_, _, _) => Unit.Default
                )
                .Subscribe(_ => RefreshUiItem())
                .AddTo(this);
            ViewModel.IsTimelinePlaying
                .Subscribe(isPlaying => playPauseImage.sprite = isPlaying ? pauseSprite : playSprite)
                .AddTo(this);


            posAccuracyField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetPosAccuracy)
                .AddTo(this);
            posMagnetToggle
                .OnValueChangedAsObservable()
                .Subscribe(ViewModel.SetPosMagnet)
                .AddTo(this);
            beatAccuracyField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetBeatAccuracy)
                .AddTo(this);
            minusBeatAccuracyButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.MinusBeatAccuracy())
                .AddTo(this);
            addBeatAccuracyButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddBeatAccuracy())
                .AddTo(this);
            beatZoomField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetBeatZoom)
                .AddTo(this);
            zoomOutButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ZoomOut())
                .AddTo(this);
            zoomInButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ZoomIn())
                .AddTo(this);
            playbackSpeedField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetPlaybackSpeed)
                .AddTo(this);
            minusPlaybackSpeedButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.MinusPlaybackSpeed())
                .AddTo(this);
            addPlaybackSpeedButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddPlaybackSpeed())
                .AddTo(this);

            slider
                .OnValueChangedAsObservable()
                .Subscribe(value =>
                {
                    if (ViewModel.AudioClipHandler.CurrentValue?.Asset == null || firstMusicOffset.CurrentValue == null)
                        return; // 首次进入时等待加载

                    var musicMsLength = ViewModel.AudioClipHandler.CurrentValue.Asset.length * 1000f;
                    var offset = (int)firstMusicOffset.CurrentValue;

                    isTimelineTimeChangeBySlider = true;
                    ViewModel.SetTimeLineTime((int)(value * (musicMsLength + offset)));
                    isTimelineTimeChangeBySlider = false;
                })
                .AddTo(this);
            playPauseButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.SwitchPlayStaus())
                .AddTo(this);
        }

        private void RefreshUiItem()
        {
            if (ViewModel.AudioClipHandler.CurrentValue?.Asset == null) // 考虑到首次进入时 asset 可能还在加载，故只检查 handler
            {
                slider.SetValueWithoutNotify(0);
                musicTimeText.text = "未设置音乐";
                return;
            }

            if (ViewModel.BpmGroup.Count <= 0)
            {
                // 目前能保证至少存在一个 bpm item，理论上不会触发，能触发就是有问题
                throw new InvalidOperationException("Bpm item 数量小于等于 0，请检查。");
                // slider.SetValueWithoutNotify(0);
                // musicTimeText.text = "未设置 BPM";
                // return;
            }

            int offset = (int)firstMusicOffset.CurrentValue!;

            if (!isTimelineTimeChangeBySlider)
            {
                float musicMsLength = ViewModel.AudioClipHandler.CurrentValue.Asset.length * 1000f;
                slider.SetValueWithoutNotify(ViewModel.CurrentTimelineTimeMs.CurrentValue / (musicMsLength + offset));
            }

            int textMsTime = ViewModel.CurrentTimelineTimeMs.CurrentValue - offset;
            bool isNegative = textMsTime < 0;
            int absMs = Math.Abs(textMsTime);
            int minutes = absMs / 60000;
            int seconds = (absMs % 60000) / 1000;
            int milliseconds = absMs % 1000;
            musicTimeText.SetText( // 避免 musicTimeText.text=$""; 的高频 GC
                isNegative
                    ? "-{0:00}:{1:00}<color=#858585DD>.{2:000}</color>"
                    : "{0:00}:{1:00}<color=#858585DD>.{2:000}</color>",
                minutes,
                seconds,
                milliseconds
            );
        }
    }
}
