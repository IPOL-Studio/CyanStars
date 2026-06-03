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
        private const double ZoomStep = 0.1;

        public ReadOnlyReactiveProperty<BaseChartNoteData?> SelectedNoteData => Model.SelectedNoteData;
        public readonly ReadOnlyReactiveProperty<string> PosAccuracyString;
        public readonly ReadOnlyReactiveProperty<bool> PosMagnetState;
        public readonly ReadOnlyReactiveProperty<string> BeatAccuracyString;
        public readonly ReadOnlyReactiveProperty<string> BeatZoomString;

        public ReadOnlyReactiveProperty<bool> IsTimelinePlaying => Model.IsTimelinePlaying;
        public ReadOnlyReactiveProperty<AssetHandler<AudioClip?>?> AudioClipHandler => Model.AudioClipHandler;
        public IReadOnlyObservableList<BpmGroupItem> BpmGroup => Model.ChartPackData.CurrentValue.BpmGroup;
        public IReadOnlyObservableList<MusicVersionDataEditorModel> MusicVersions => Model.ChartPackData.CurrentValue.MusicVersions;

        public int CurrentTimelineTimeMs => Model.CurrentTimelineTimeMs;

        public EditorAttributeViewModel(ChartEditorModel model)
            : base(model)
        {
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
            // CurrentTimelineTimeMs = Model.CurrentTimelineTimeMs
            //     .ThrottleLastFrame(1)
            //     .ToReadOnlyReactiveProperty()
            //     .AddTo(base.Disposables);
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
            if (Model.BeatZoom.Value <= ZoomStep)
                return;

            Model.BeatZoom.Value -= ZoomStep;
        }

        public void ZoomIn()
        {
            Model.BeatZoom.Value += ZoomStep;
        }

        public void SetTimeLineTime(int msTime)
        {
            Model.CurrentTimelineTimeMs = msTime;
        }
    }
}
