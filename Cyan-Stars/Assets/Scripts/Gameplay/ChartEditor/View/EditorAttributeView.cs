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


        private ReadOnlyReactiveProperty<bool> frameVisibility = null!;


        public override void Bind(EditorAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            frameVisibility =
                ViewModel.SelectedNoteData
                    .Select(note => note == null)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this);

            frameVisibility
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
        }
    }
}
