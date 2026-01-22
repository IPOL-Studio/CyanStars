#nullable enable

using System.Globalization;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditorAttributeViewModel : BaseViewModel
    {
        private const int BeatAccuracyStep = 1;
        private const float ZoomStep = 0.1f;

        public readonly ReadOnlyReactiveProperty<bool> ShowEditorAttributeFrame;
        public readonly ReadOnlyReactiveProperty<string> PosAccuracyString;
        public readonly ReadOnlyReactiveProperty<bool> PosMagnetState;
        public readonly ReadOnlyReactiveProperty<string> BeatAccuracyString;
        public readonly ReadOnlyReactiveProperty<string> BeatZoomString;


        public EditorAttributeViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            ShowEditorAttributeFrame = Model.SelectedNoteData
                .Select(data => data == null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            PosAccuracyString = Model.PosAccuracy
                .Select(posAccuracy => posAccuracy.ToString())
                .ToReadOnlyReactiveProperty(Model.PosAccuracy.Value.ToString())
                .AddTo(base.Disposables);
            PosMagnetState = Model.PosMagnet
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            BeatAccuracyString = Model.BeatAccuracy
                .Select(beatAccuracy => beatAccuracy.ToString())
                .ToReadOnlyReactiveProperty(Model.BeatAccuracy.Value.ToString())
                .AddTo(base.Disposables);
            BeatZoomString = Model.BeatZoom
                .Select(beatZoom => beatZoom.ToString(CultureInfo.InvariantCulture))
                .ToReadOnlyReactiveProperty(Model.BeatZoom.Value.ToString(CultureInfo.InvariantCulture))
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
