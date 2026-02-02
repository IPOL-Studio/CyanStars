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
        private GameObject editorAttributeFrame = null!;

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


        public override void Bind(EditorAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.ShowEditorAttributeFrame
                .Subscribe(value => editorAttributeFrame.SetActive(value))
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

            posAccuracyField.onEndEdit.AddListener(ViewModel.SetPosAccuracy);
            posMagnetToggle.onValueChanged.AddListener(ViewModel.SetPosMagnet);
            beatAccuracyField.onEndEdit.AddListener(ViewModel.SetBeatAccuracy);
            minusBeatAccuracyButton.onClick.AddListener(ViewModel.MinusBeatAccuracy);
            addBeatAccuracyButton.onClick.AddListener(ViewModel.AddBeatAccuracy);
            beatZoomField.onEndEdit.AddListener(ViewModel.SetBeatZoom);
            zoomOutButton.onClick.AddListener(ViewModel.ZoomOut);
            zoomInButton.onClick.AddListener(ViewModel.ZoomIn);
        }


        protected override void OnDestroy()
        {
            posAccuracyField.onEndEdit.RemoveListener(ViewModel.SetPosAccuracy);
            posMagnetToggle.onValueChanged.RemoveListener(ViewModel.SetPosMagnet);
            beatAccuracyField.onEndEdit.RemoveListener(ViewModel.SetBeatAccuracy);
            minusBeatAccuracyButton.onClick.RemoveListener(ViewModel.MinusBeatAccuracy);
            addBeatAccuracyButton.onClick.RemoveListener(ViewModel.AddBeatAccuracy);
            beatZoomField.onEndEdit.RemoveListener(ViewModel.SetBeatZoom);
            zoomOutButton.onClick.RemoveListener(ViewModel.ZoomOut);
            zoomInButton.onClick.RemoveListener(ViewModel.ZoomIn);
        }
    }
}
