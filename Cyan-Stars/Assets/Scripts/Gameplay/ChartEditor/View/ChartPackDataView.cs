#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
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
        private TMP_Text coverPathText = null!; // TODO

        [SerializeField]
        private Button importCoverButton = null!; // TODO

        [SerializeField]
        private Button exportChartPackButton = null!; //TODO


        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.CanvasVisible.OnValueChanged += SetCanvasVisible;
            ViewModel.ChartPackTitle.OnValueChanged += SetTitleField;
            ViewModel.PreviewStartBeatField1String.OnValueChanged += SetPreviewStartBeatField1;
            ViewModel.PreviewStartBeatField2String.OnValueChanged += SetPreviewStartBeatField2;
            ViewModel.PreviewStartBeatField3String.OnValueChanged += SetPreviewStartBeatField3;
            ViewModel.PreviewEndBeatField1String.OnValueChanged += SetPreviewEndBeatField1;
            ViewModel.PreviewEndBeatField2String.OnValueChanged += SetPreviewEndBeatField2;
            ViewModel.PreviewEndBeatField3String.OnValueChanged += SetPreviewEndBeatField3;

            closeCanvasButton.onClick.AddListener(ViewModel.CloseCanvas);
            previewStartBeatField1.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
                    previewStartBeatField1.text,
                    previewStartBeatField2.text,
                    previewStartBeatField3.text
                )
            );
            previewStartBeatField2.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
                    previewStartBeatField1.text,
                    previewStartBeatField2.text,
                    previewStartBeatField3.text
                )
            );
            previewStartBeatField3.onEndEdit.AddListener(_ =>
                ViewModel.SetPreviewEndBeat(
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
        }

        private void SetCanvasVisible(bool visible)
        {
            canvas.enabled = visible;
        }

        private void SetTitleField(string title)
        {
            chartPackTitleField.text = title;
        }

        private void SetPreviewStartBeatField1(string text)
        {
            previewStartBeatField1.text = text;
        }

        private void SetPreviewStartBeatField2(string text)
        {
            previewStartBeatField2.text = text;
        }

        private void SetPreviewStartBeatField3(string text)
        {
            previewStartBeatField3.text = text;
        }

        private void SetPreviewEndBeatField1(string text)
        {
            previewEndBeatField1.text = text;
        }

        private void SetPreviewEndBeatField2(string text)
        {
            previewEndBeatField2.text = text;
        }

        private void SetPreviewEndBeatField3(string text)
        {
            previewEndBeatField3.text = text;
        }

        protected override void OnDestroy()
        {
            ViewModel.CanvasVisible.OnValueChanged -= SetCanvasVisible;
            ViewModel.ChartPackTitle.OnValueChanged -= SetTitleField;
            ViewModel.PreviewStartBeatField1String.OnValueChanged -= SetPreviewStartBeatField1;
            ViewModel.PreviewStartBeatField2String.OnValueChanged -= SetPreviewStartBeatField2;
            ViewModel.PreviewStartBeatField3String.OnValueChanged -= SetPreviewStartBeatField3;
            ViewModel.PreviewEndBeatField1String.OnValueChanged -= SetPreviewEndBeatField1;
            ViewModel.PreviewEndBeatField2String.OnValueChanged -= SetPreviewEndBeatField2;
            ViewModel.PreviewEndBeatField3String.OnValueChanged -= SetPreviewEndBeatField3;

            closeCanvasButton.onClick.RemoveAllListeners();
            closeCanvasButton.onClick.RemoveAllListeners();
            previewStartBeatField1.onEndEdit.RemoveAllListeners();
            previewStartBeatField2.onEndEdit.RemoveAllListeners();
            previewStartBeatField3.onEndEdit.RemoveAllListeners();
            previewEndBeatField1.onEndEdit.RemoveAllListeners();
            previewEndBeatField2.onEndEdit.RemoveAllListeners();
            previewEndBeatField3.onEndEdit.RemoveAllListeners();
        }
    }
}
