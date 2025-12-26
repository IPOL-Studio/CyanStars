#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
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

            posAccuracyField.onEndEdit.AddListener(ViewModel.SetPosAccuracy);
            posMagnetToggle.onValueChanged.AddListener(ViewModel.SetPosMagnet);
            beatAccuracyField.onEndEdit.AddListener(ViewModel.SetBeatAccuracy);
            beatZoomField.onEndEdit.AddListener(ViewModel.SetBeatZoom);
        }


        protected override void OnDestroy()
        {
            posAccuracyField.onEndEdit.RemoveAllListeners();
            posMagnetToggle.onValueChanged.RemoveAllListeners();
            beatAccuracyField.onEndEdit.RemoveAllListeners();
            beatZoomField.onEndEdit.RemoveAllListeners();
        }
    }
}
