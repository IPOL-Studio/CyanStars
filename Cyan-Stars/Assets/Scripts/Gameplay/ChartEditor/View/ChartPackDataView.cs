#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataView : BaseView<ChartPackDataViewModel>
    {
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

        [SerializeField]
        private TMP_InputField chartPackTitleField = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField1 = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField2 = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField3 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField1 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField2 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField3 = null!;

        [SerializeField]
        private TMP_Text coverPathText = null!;

        [SerializeField]
        private GameObject coverCropFrameObject = null!;

        [SerializeField]
        private Button exportChartPackButton = null!; //TODO


        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.CanvasVisible
                .Subscribe(visible =>
                {
                    canvas.enabled = visible;
                })
                .AddTo(this);
            ViewModel.ChartPackTitle
                .Subscribe(title =>
                {
                    chartPackTitleField.text = title;
                })
                .AddTo(this);

            ViewModel.PreviewStartBeatField1String
                .Subscribe(text =>
                {
                    previewStartBeatField1.text = text;
                })
                .AddTo(this);
            ViewModel.PreviewStartBeatField2String.Subscribe(text =>
                {
                    previewStartBeatField2.text = text;
                })
                .AddTo(this);
            ViewModel.PreviewStartBeatField3String.Subscribe(text =>
                {
                    previewStartBeatField3.text = text;
                })
                .AddTo(this);
            ViewModel.PreviewEndBeatField1String.Subscribe(text =>
                {
                    previewEndBeatField1.text = text;
                })
                .AddTo(this);
            ViewModel.PreviewEndBeatField2String.Subscribe(text =>
                {
                    previewEndBeatField2.text = text;
                })
                .AddTo(this);
            ViewModel.PreviewEndBeatField3String.Subscribe(text =>
                {
                    previewEndBeatField3.text = text;
                })
                .AddTo(this);

            ViewModel.CoverFilePathString
                .Subscribe(text =>
                {
                    coverPathText.text = text;
                })
                .AddTo(this);
            ViewModel.CoverCropAreaVisible
                .Subscribe(isVisible => coverCropFrameObject.SetActive(isVisible))
                .AddTo(this);


            closeCanvasButton.onClick.AddListener(ViewModel.CloseCanvas);
            chartPackTitleField.onEndEdit.AddListener(ViewModel.SetChartPackTitle);
            previewStartBeatField1.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewStartBeat(
                    previewStartBeatField1.text,
                    previewStartBeatField2.text,
                    previewStartBeatField3.text
                )
            );
            previewStartBeatField2.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewStartBeat(
                    previewStartBeatField1.text,
                    previewStartBeatField2.text,
                    previewStartBeatField3.text
                )
            );
            previewStartBeatField3.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewStartBeat(
                    previewStartBeatField1.text,
                    previewStartBeatField2.text,
                    previewStartBeatField3.text
                )
            );
            previewEndBeatField1.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
                    previewEndBeatField1.text,
                    previewEndBeatField2.text,
                    previewEndBeatField3.text
                )
            );
            previewEndBeatField2.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
                    previewEndBeatField1.text,
                    previewEndBeatField2.text,
                    previewEndBeatField3.text
                )
            );
            previewEndBeatField3.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
                    previewEndBeatField1.text,
                    previewEndBeatField2.text,
                    previewEndBeatField3.text
                )
            );

            exportChartPackButton.onClick.AddListener(ViewModel.ExportChartPack);
        }

        public void OpenCanvas()
        {
            ViewModel.OpenCanvas();
        }

        protected override void OnDestroy()
        {
            closeCanvasButton.onClick.RemoveAllListeners();
            chartPackTitleField.onEndEdit.RemoveAllListeners();
            previewStartBeatField1.onEndEdit.RemoveAllListeners();
            previewStartBeatField2.onEndEdit.RemoveAllListeners();
            previewStartBeatField3.onEndEdit.RemoveAllListeners();
            previewEndBeatField1.onEndEdit.RemoveAllListeners();
            previewEndBeatField2.onEndEdit.RemoveAllListeners();
            previewEndBeatField3.onEndEdit.RemoveAllListeners();
            exportChartPackButton.onClick.RemoveAllListeners();
        }
    }
}
