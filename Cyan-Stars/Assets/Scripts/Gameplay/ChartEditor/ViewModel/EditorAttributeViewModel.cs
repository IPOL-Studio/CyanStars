#nullable enable

using System.Globalization;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditorAttributeViewModel : BaseViewModel
    {
        private readonly BindableProperty<string> posAccuracyString;
        private readonly BindableProperty<bool> posMagnetState;
        private readonly BindableProperty<string> beatAccuracyString;
        private readonly BindableProperty<string> beatZoomString;

        public IReadonlyBindableProperty<string> PosAccuracyString => posAccuracyString;
        public IReadonlyBindableProperty<bool> PosMagnetState => posMagnetState;
        public IReadonlyBindableProperty<string> BeatAccuracyString => beatAccuracyString;
        public IReadonlyBindableProperty<string> BeatZoomString => beatZoomString;


        public EditorAttributeViewModel(ChartEditorModel model, CommandManager commandManager) : base(model, commandManager)
        {
            posAccuracyString = new BindableProperty<string>(Model.PosAccuracy.Value.ToString());
            posMagnetState = new BindableProperty<bool>(Model.PosMagnet.Value);
            beatAccuracyString = new BindableProperty<string>(Model.BeatAccuracy.Value.ToString());
            beatZoomString = new BindableProperty<string>(Model.BeatZoom.Value.ToString(CultureInfo.InvariantCulture));

            Model.PosAccuracy.OnValueChanged += value => posAccuracyString.Value = value.ToString();
            Model.PosMagnet.OnValueChanged += value => posMagnetState.Value = value;
            Model.BeatAccuracy.OnValueChanged += value => beatAccuracyString.Value = value.ToString();
            Model.BeatZoom.OnValueChanged += value => beatZoomString.Value = value.ToString(CultureInfo.InvariantCulture);
        }

        public void SetPosAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy))
            {
                posAccuracyString.ForceNotify();
                return;
            }

            posAccuracyString.Value = accuracy.ToString();
            Model.PosAccuracy.Value = accuracy;
        }

        public void SetPosMagnet(bool isMagnetOn)
        {
            posMagnetState.Value = isMagnetOn;
            Model.PosMagnet.Value = isMagnetOn;
        }

        public void SetBeatAccuracy(string accuracyString)
        {
            if (!int.TryParse(accuracyString, out int accuracy))
            {
                beatAccuracyString.ForceNotify();
                return;
            }

            beatAccuracyString.Value = accuracy.ToString();
            Model.BeatAccuracy.Value = accuracy;
        }

        public void SetBeatZoom(string zoomString)
        {
            if (!float.TryParse(zoomString, out float zoom))
            {
                beatZoomString.ForceNotify();
                return;
            }

            beatZoomString.Value = zoom.ToString(CultureInfo.InvariantCulture);
            Model.BeatZoom.Value = zoom;
        }
    }
}
