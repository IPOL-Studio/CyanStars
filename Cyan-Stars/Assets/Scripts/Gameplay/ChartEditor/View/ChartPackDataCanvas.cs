#nullable enable

using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class ChartPackDataCanvas : BaseView
    {
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeCanvaButton = null!;

        [SerializeField]
        private GameObject contentObject = null!;

        [Header("谱包标题、预览拍、曲绘路径")]
        [SerializeField]
        private TMP_InputField chartPackTitleField = null!;

        [SerializeField]
        private TMP_InputField previewStartField1 = null!;

        [SerializeField]
        private TMP_InputField previewStartField2 = null!;

        [SerializeField]
        private TMP_InputField previewStartField3 = null!;

        [SerializeField]
        private TMP_InputField previewEndField1 = null!;

        [SerializeField]
        private TMP_InputField previewEndField2 = null!;

        [SerializeField]
        private TMP_InputField previewEndField3 = null!;

        [SerializeField]
        private TMP_Text coverPath = null!;

        [SerializeField]
        private Button importCoverButton = null!;

        [Header("曲绘预览和裁剪")]
        [SerializeField]
        private GameObject chartCropFrame = null!;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter = null!;

        [SerializeField]
        private RawImage backRawImage = null!;

        [SerializeField]
        private RawImage topRawImage = null!;

        [SerializeField]
        private RectMask2D rectMask = null!;

        [SerializeField]
        private RectTransform imageFrameRect = null!;

        [SerializeField]
        private RectTransform coverCropAreaRect = null!;

        [Header("导出")]
        [SerializeField]
        private Button exportChartPackButton = null!; // TODO


        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            Model.OnChartPackDataChanged += RefreshUI;
            Model.OnChartPackDataCanvasVisiblenessChanged += RefreshUI;

            closeCanvaButton.onClick.AddListener(() => { Model.SetChartPackDataCanvasVisibleness(false); });

            chartPackTitleField.onEndEdit.AddListener(text => { Model.UpdateChartPackTitle(text); });
            previewStartField1.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewStartField2.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewStartField3.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewStareBeat(previewStartField1.text, previewStartField2.text,
                    previewStartField3.text);
            });
            previewEndField1.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField2.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField3.onEndEdit.AddListener(_ =>
            {
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });

            importCoverButton.onClick.AddListener(() =>
            {
                GameRoot.File.OpenLoadFilePathBrowser(async path => { await Model.UpdateCoverFilePath(path); },
                    filters: new[] { GameRoot.File.SpriteFilter }
                );
            });

            // TODO: 导出谱包
        }


        private void RefreshUI()
        {
            canvas.enabled = Model.ChartPackDataCanvasVisibleness;

            // 刷新标题和预览拍输入框
            chartPackTitleField.text = Model.ChartPackData.Title;
            previewStartField1.text = Model.ChartPackData.MusicPreviewStartBeat.IntegerPart.ToString();
            previewStartField2.text = Model.ChartPackData.MusicPreviewStartBeat.Numerator.ToString();
            previewStartField3.text = Model.ChartPackData.MusicPreviewStartBeat.Denominator.ToString();
            previewEndField1.text = Model.ChartPackData.MusicPreviewEndBeat.IntegerPart.ToString();
            previewEndField2.text = Model.ChartPackData.MusicPreviewEndBeat.Numerator.ToString();
            previewEndField3.text = Model.ChartPackData.MusicPreviewEndBeat.Denominator.ToString();

            // 刷新曲绘路径、预览区域可见性、图片材质
            coverPath.text = Model.ChartPackData.CoverFilePath;
            chartCropFrame.SetActive(Model.ChartPackData.CoverFilePath != null);
            backRawImage.texture = Model.CoverTexture;
            topRawImage.texture = Model.CoverTexture;

            // 刷新曲绘高度
            aspectRatioFitter.aspectRatio = Model.CoverTexture != null
                ? (float)Model.CoverTexture.width / Model.CoverTexture.height
                : Mathf.Infinity;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentObject.GetComponent<RectTransform>());

            // 刷新曲绘裁剪框位置
            float startX = Model.CoverTexture != null
                ? Model.ChartPackData.CropStartPosition.x * imageFrameRect.rect.width / Model.CoverTexture.width
                : 0;
            float startY = Model.CoverTexture != null
                ? Model.ChartPackData.CropStartPosition.y * imageFrameRect.rect.height / Model.CoverTexture.height
                : 0;
            startX = Mathf.Max(0, Mathf.Min(startX, imageFrameRect.rect.width));
            startY = Mathf.Max(0, Mathf.Min(startY, imageFrameRect.rect.height));
            coverCropAreaRect.anchoredPosition = new Vector2(startX, startY);

            float width = Model.CoverTexture != null
                ? Model.ChartPackData.CropWidth * imageFrameRect.rect.width / Model.CoverTexture.width
                : 0;
            float height = width / 4;
            width = Mathf.Max(0, Mathf.Min(width, imageFrameRect.rect.width - startX));
            height = Mathf.Max(0, Mathf.Min(height, imageFrameRect.rect.height - startY));
            coverCropAreaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            coverCropAreaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width);

            // 刷新曲绘裁剪遮罩位置
            rectMask.padding = new Vector4(
                startX,
                startY,
                imageFrameRect.rect.width - width - startX,
                imageFrameRect.rect.height - height - startY
            );
        }

        private void OnDestroy()
        {
            Model.OnChartPackDataChanged -= RefreshUI;
            Model.OnChartPackDataCanvasVisiblenessChanged -= RefreshUI;
        }
    }
}
