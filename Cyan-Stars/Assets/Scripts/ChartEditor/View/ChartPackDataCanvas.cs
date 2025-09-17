using CyanStars.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class ChartPackDataCanvas : BaseView
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private Button closeCanvaButton;


        [SerializeField]
        private TMP_InputField chartPackTitleField;

        [SerializeField]
        private TMP_InputField previewStartField1;

        [SerializeField]
        private TMP_InputField previewStartField2;

        [SerializeField]
        private TMP_InputField previewStartField3;

        [SerializeField]
        private TMP_InputField previewEndField1;

        [SerializeField]
        private TMP_InputField previewEndField2;

        [SerializeField]
        private TMP_InputField previewEndField3;

        [SerializeField]
        private TMP_Text coverPath; // TODO

        [SerializeField]
        private Button importCoverButton; // TODO

        [SerializeField]
        private GameObject coverCropFrame; // TODO

        [SerializeField]
        private Button exportChartPackButton; // TODO


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            Model.OnChartPackDataChanged += RefreshUI;

            closeCanvaButton.onClick.AddListener(() => { Model.SetMusicVersionCanvasVisibleness(false); });

            chartPackTitleField.onEndEdit.AddListener((text) => { Model.UpdateChartPackTitle(text); });
            previewStartField1.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewStartField2.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewStartField3.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewEndField1.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField2.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField3.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewStareBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
        }

        private void RefreshUI()
        {
            canvas.enabled = Model.ChartPackDataCanvasVisibleness;
            chartPackTitleField.text = Model.ChartPackData.Title;
            previewStartField1.text = Model.ChartPackData.MusicPreviewStartBeat.IntegerPart.ToString();
            previewStartField2.text = Model.ChartPackData.MusicPreviewStartBeat.Numerator.ToString();
            previewStartField3.text = Model.ChartPackData.MusicPreviewStartBeat.Denominator.ToString();
            previewEndField1.text = Model.ChartPackData.MusicPreviewEndBeat.IntegerPart.ToString();
            previewEndField2.text = Model.ChartPackData.MusicPreviewEndBeat.Numerator.ToString();
            previewEndField3.text = Model.ChartPackData.MusicPreviewEndBeat.Denominator.ToString();
            coverPath.text = Model.ChartPackData.CoverFilePath;
        }

        private void OnDestroy()
        {
            Model.OnChartPackDataChanged -= RefreshUI;
        }
    }
}
