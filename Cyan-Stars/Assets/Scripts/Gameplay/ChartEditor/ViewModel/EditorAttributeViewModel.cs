#nullable enable

using System.Globalization;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditorAttributeViewModel : BaseViewModel
    {
        private const int BeatAccuracyStep = 1;
        private const double BeatZoomStep = 0.1;
        private const double PlaybackSpeedStep = 0.1;

        public ReadOnlyReactiveProperty<BaseChartNoteData?> SelectedNoteData => Model.SelectedNoteData;

        public readonly ReadOnlyReactiveProperty<bool> IsAbleToMinusBeatAccuracy;
        public readonly ReadOnlyReactiveProperty<bool> IsAbleToMinusBeatZoom;
        public readonly ReadOnlyReactiveProperty<bool> IsAbleToMinusPlaybackSpeed;

        // TODO: 将这堆 <string> 改为 <int> 或 <float>，由 View 负责转换
        public readonly ReadOnlyReactiveProperty<string> PosAccuracyString;
        public readonly ReadOnlyReactiveProperty<bool> PosMagnetState;
        public readonly ReadOnlyReactiveProperty<string> BeatAccuracyString;
        public readonly ReadOnlyReactiveProperty<string> BeatZoomString;
        public readonly ReadOnlyReactiveProperty<double> PlaybackSpeed;

        public ReadOnlyReactiveProperty<bool> IsTimelinePlaying => Model.IsTimelinePlaying;
        public ReadOnlyReactiveProperty<int> CurrentTimelineTimeMs => Model.CurrentTimelineTimeMs;
        public ReadOnlyReactiveProperty<AssetHandler<AudioClip?>?> AudioClipHandler => Model.AudioClipHandler;
        public IReadOnlyObservableList<BpmGroupItem> BpmGroup => Model.ChartPackData.CurrentValue.BpmGroup;
        public IReadOnlyObservableList<MusicVersionDataEditorModel> MusicVersions => Model.ChartPackData.CurrentValue.MusicVersions;


        public EditorAttributeViewModel(ChartEditorModel model)
            : base(model)
        {
            IsAbleToMinusBeatAccuracy = Model.BeatAccuracy
                .Select(beatAccuracy => beatAccuracy > BeatAccuracyStep)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            IsAbleToMinusBeatZoom = Model.BeatZoom
                .Select(beatZoom => beatZoom > BeatZoomStep)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            IsAbleToMinusPlaybackSpeed = Model.PlaybackSpeed
                .Select(playbackSpeed => playbackSpeed > PlaybackSpeedStep)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            // TODO: 优化此处属性赋值，直接透传数据并由 View 自行组合和处理逻辑
            PosAccuracyString = Model.PosAccuracy
                .Select(posAccuracy => posAccuracy.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, Model.PosAccuracy.Value.ToString())
                .AddTo(base.Disposables);
            PosMagnetState = Model.PosMagnet
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            BeatAccuracyString = Model.BeatAccuracy
                .Select(beatAccuracy => beatAccuracy.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, Model.BeatAccuracy.Value.ToString())
                .AddTo(base.Disposables);
            BeatZoomString = Model.BeatZoom
                .Select(beatZoom => beatZoom.ToString("0.##", CultureInfo.InvariantCulture))
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, Model.BeatZoom.Value.ToString("0.##", CultureInfo.InvariantCulture))
                .AddTo(base.Disposables);
            PlaybackSpeed = Model.PlaybackSpeed
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<double>.Instance)
                .AddTo(base.Disposables);
        }

        public void SetPosAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy) || accuracy < 0)
            {
                Model.PosAccuracy.ForceNotify();
                return;
            }

            Model.PosAccuracy.Value = accuracy;
        }

        public void SetPosMagnet(bool isMagnetOn)
        {
            Model.PosMagnet.Value = isMagnetOn;
        }

        public void SetBeatAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy) || accuracy < 1)
            {
                Model.BeatAccuracy.ForceNotify();
                return;
            }

            Model.BeatAccuracy.Value = accuracy;
        }

        public void MinusBeatAccuracy()
        {
            if (Model.BeatAccuracy.Value <= BeatAccuracyStep)
                return;

            Model.BeatAccuracy.Value -= BeatAccuracyStep;
        }

        public void AddBeatAccuracy()
        {
            Model.BeatAccuracy.Value += BeatAccuracyStep;
        }

        public void SetBeatZoom(string zoomString)
        {
            if (!float.TryParse(zoomString, NumberStyles.Any, CultureInfo.InvariantCulture, out float zoom) || zoom <= 0f)
            {
                Model.BeatZoom.ForceNotify();
                return;
            }

            Model.BeatZoom.Value = zoom;
        }

        public void ZoomOut()
        {
            if (Model.BeatZoom.Value <= BeatZoomStep)
                return;

            Model.BeatZoom.Value -= BeatZoomStep;
        }

        public void ZoomIn()
        {
            Model.BeatZoom.Value += BeatZoomStep;
        }

        public void SetPlaybackSpeed(string speedString)
        {
            if (!double.TryParse(speedString, out double speed) || speed <= 0)
            {
                Model.PlaybackSpeed.ForceNotify();
                return;
            }

            Model.PlaybackSpeed.Value = speed;
        }

        public void MinusPlaybackSpeed()
        {
            if (PlaybackSpeed.CurrentValue <= PlaybackSpeedStep)
                return;

            Model.PlaybackSpeed.Value -= PlaybackSpeedStep;
        }

        public void AddPlaybackSpeed()
        {
            Model.PlaybackSpeed.Value += PlaybackSpeedStep;
        }

        public void SetTimeLineTime(int msTime)
        {
            Model.CurrentTimelineTimeMs.Value = msTime;
        }

        public void SwitchPlayStaus()
        {
            Model.IsTimelinePlaying.Value = !Model.IsTimelinePlaying.CurrentValue;
        }
    }
}
