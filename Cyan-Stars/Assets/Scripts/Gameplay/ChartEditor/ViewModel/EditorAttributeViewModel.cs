#nullable enable

using System.Globalization;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditorAttributeViewModel : BaseViewModel
    {
        public readonly BindableProperty<string> PosAccuracyString = new BindableProperty<string>();
        public readonly BindableProperty<bool> PosMagnetState = new BindableProperty<bool>();
        public readonly BindableProperty<string> BeatAccuracyString = new BindableProperty<string>();
        public readonly BindableProperty<string> BeatZoomString = new BindableProperty<string>();


        public EditorAttributeViewModel(ChartEditorModel model, CommandManager commandManager) : base(model, commandManager)
        {
            PosAccuracyString.Value = Model.PosAccuracy.Value.ToString();
            PosMagnetState.Value = Model.PosMagnet.Value;
            BeatAccuracyString.Value = Model.BeatAccuracy.Value.ToString();
            BeatZoomString.Value = Model.BeatZoom.Value.ToString(CultureInfo.InvariantCulture);

            Model.PosAccuracy.OnValueChanged += value => PosAccuracyString.Value = value.ToString();
            Model.PosMagnet.OnValueChanged += value => PosMagnetState.Value = value;
            Model.BeatAccuracy.OnValueChanged += value => BeatAccuracyString.Value = value.ToString();
            Model.BeatZoom.OnValueChanged += value => BeatZoomString.Value = value.ToString(CultureInfo.InvariantCulture);
        }

        public void SetPosAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy))
            {
                PosAccuracyString.ForceNotify();
                return;
            }

            PosAccuracyString.Value = accuracy.ToString();
            Model.PosAccuracy.Value = accuracy;
        }

        public void SetPosMagnet(bool isMagnetOn)
        {
            PosMagnetState.Value = isMagnetOn;
            Model.PosMagnet.Value = isMagnetOn;
        }

        public void SetBeatAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy))
            {
                BeatAccuracyString.ForceNotify();
                return;
            }

            BeatAccuracyString.Value = accuracy.ToString();
            Model.BeatAccuracy.Value = accuracy;
        }

        public void SetBeatZoom(string zoomString)
        {
            if (!float.TryParse(zoomString, out float zoom))
            {
                BeatZoomString.ForceNotify();
                return;
            }

            BeatZoomString.Value = zoom.ToString(CultureInfo.InvariantCulture);
            Model.BeatZoom.Value = zoom;
        }
    }
}
