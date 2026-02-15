#nullable enable

using System.Globalization;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

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
    }
}
