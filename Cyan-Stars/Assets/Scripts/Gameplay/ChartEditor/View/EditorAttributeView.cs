#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditorAttributeView : BaseView<EditorAttributeViewModel>
    {
        [SerializeField]
        private TMP_InputField posAccuracyField = null!;

        [SerializeField]
        private Toggle posMagnetToggle = null!;

        [SerializeField]
        private TMP_InputField beatAccuracyField = null!;

        [SerializeField]
        private TMP_InputField beatZoomField = null!;


        public override void Bind(EditorAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            posAccuracyField.text = ViewModel.PosAccuracyString.Value;
            posMagnetToggle.isOn = ViewModel.PosMagnetState.Value;
            beatAccuracyField.text = ViewModel.BeatAccuracyString.Value;
            beatZoomField.text = ViewModel.BeatZoomString.Value;

            ViewModel.PosAccuracyString.OnValueChanged += OnPosAccuracyFieldChanged;
            ViewModel.PosMagnetState.OnValueChanged += OnPosMagnetChanged;
            ViewModel.BeatAccuracyString.OnValueChanged += OnBeatAccuracyFieldChanged;
            ViewModel.BeatZoomString.OnValueChanged += OnBeatZoomFieldChanged;

            posAccuracyField.onEndEdit.AddListener(value => ViewModel.SetPosAccuracy(value));
            posMagnetToggle.onValueChanged.AddListener(value => ViewModel.SetPosMagnet(value));
            beatAccuracyField.onEndEdit.AddListener(value => ViewModel.SetBeatAccuracy(value));
            beatZoomField.onEndEdit.AddListener(value => ViewModel.SetBeatZoom(value));
        }

        private void OnPosAccuracyFieldChanged(string value)
        {
            posAccuracyField.text = value;
        }

        private void OnPosMagnetChanged(bool value)
        {
            posMagnetToggle.isOn = value;
        }

        private void OnBeatAccuracyFieldChanged(string value)
        {
            beatAccuracyField.text = value;
        }

        private void OnBeatZoomFieldChanged(string value)
        {
            beatZoomField.text = value;
        }

        protected override void OnDestroy()
        {
            ViewModel.PosAccuracyString.OnValueChanged -= OnPosAccuracyFieldChanged;
            ViewModel.PosMagnetState.OnValueChanged -= OnPosMagnetChanged;
            ViewModel.BeatAccuracyString.OnValueChanged -= OnBeatAccuracyFieldChanged;
            ViewModel.BeatZoomString.OnValueChanged -= OnBeatZoomFieldChanged;

            posAccuracyField.onEndEdit.RemoveAllListeners();
            posMagnetToggle.onValueChanged.RemoveAllListeners();
            beatAccuracyField.onEndEdit.RemoveAllListeners();
            beatZoomField.onEndEdit.RemoveAllListeners();
        }
    }
}
