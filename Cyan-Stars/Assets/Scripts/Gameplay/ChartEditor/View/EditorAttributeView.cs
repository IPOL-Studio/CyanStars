#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditorAttributeView : BaseView<EditorAttributeViewModel>
    {
        [Header("编辑器属性")]
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

        [Header("进度条")]
        [SerializeField]
        private Slider slider = null!;

        [SerializeField]
        private TMP_Text musicTimeText = null!;

        [SerializeField]
        private Button playPauseButton = null!;


        private ReadOnlyReactiveProperty<bool> frameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> isTimeLineReadyToPlay = null!; // true == 已加载音乐，bpm 组有至少 1 个 item，music 组有至少 1 个 item
        private ReadOnlyReactiveProperty<int?> firstMusicOffset = null!; // 缓存的首个音乐的可观察 offset


        public override void Bind(EditorAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

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

            isTimeLineReadyToPlay = Observable
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

            Observable.CombineLatest(
                    ViewModel.IsTimelinePlaying,
                    isTimeLineReadyToPlay,
                    (isPlaying, isReadyToPlay) => !isPlaying && isReadyToPlay
                )
                .Subscribe(interactable => slider.interactable = interactable)
                .AddTo(this);
            firstMusicOffset
                .Subscribe(_ => RefreshUiItem())
                .AddTo(this);
            ViewModel.AudioClipHandler
                .Subscribe(_ => RefreshUiItem())
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

            slider
                .OnValueChangedAsObservable()
                .Subscribe(value =>
                {
                    if (ViewModel.AudioClipHandler.CurrentValue?.Asset == null || firstMusicOffset.CurrentValue == null)
                        return; // 首次进入时等待加载

                    var musicMsLength = ViewModel.AudioClipHandler.CurrentValue.Asset.length * 1000f;
                    var offset = (int)firstMusicOffset.CurrentValue;
                    ViewModel.SetTimeLineTime((int)(value * (musicMsLength + offset)));
                })
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
                throw new Exception();
                // slider.SetValueWithoutNotify(0);
                // musicTimeText.text = "未设置 BPM";
                // return;
            }

            float musicMsLength = ViewModel.AudioClipHandler.CurrentValue.Asset.length * 1000f;
            int offset = (int)firstMusicOffset.CurrentValue!;
            slider.SetValueWithoutNotify(ViewModel.CurrentTimelineTimeMs / (musicMsLength + offset));

            int textMsTime = ViewModel.CurrentTimelineTimeMs - offset;
            bool isNegative = textMsTime < 0;
            int absMs = Math.Abs(textMsTime);
            int minutes = absMs / 60000;
            int seconds = (absMs % 60000) / 1000;
            int milliseconds = absMs % 1000;
            string sign = isNegative ? "-" : "";
            musicTimeText.text = $"{sign}{minutes:D2}:{seconds:D2}<color=#858585DD>.{milliseconds:D3}</color>";
        }

        private void Update()
        {
            if (ViewModel.IsTimelinePlaying.CurrentValue)
                RefreshUiItem();
        }
    }
}
