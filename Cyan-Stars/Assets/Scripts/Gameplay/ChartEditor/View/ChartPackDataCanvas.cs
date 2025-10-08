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
        private TMP_Text coverPath;

        [SerializeField]
        private Button importCoverButton;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;

        [SerializeField]
        private Image backImage;

        [SerializeField]
        private Image topImage;

        [SerializeField]
        private RectMask2D rectMask;

        [SerializeField]
        private RectTransform imageFrameRect;

        [SerializeField]
        private RectTransform coverCropAreaRect;

        [SerializeField]
        private Button exportChartPackButton; // TODO


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            Model.OnChartPackDataChanged += RefreshUI;
            Model.OnChartPackDataCanvasVisiblenessChanged += RefreshUI;

            closeCanvaButton.onClick.AddListener(() => { Model.SetChartPackDataCanvasVisibleness(false); });

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
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField2.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });
            previewEndField3.onEndEdit.AddListener((_) =>
            {
                Model.UpdatePreviewEndBeat(previewEndField1.text, previewEndField2.text, previewEndField3.text);
            });

            importCoverButton.onClick.AddListener(() =>
            {
                GameRoot.File.OpenLoadFilePathBrowser(
                    (path) => { Model.UpdateCoverFilePath(path); },
                    filters: new[] { GameRoot.File.SpriteFilter }
                );
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
            backImage.sprite = Model.CoverSprite;
            topImage.sprite = Model.CoverSprite;
            aspectRatioFitter.aspectRatio = Model.CoverSprite != null
                ? Model.CoverSprite.rect.width / Model.CoverSprite.rect.height
                : Mathf.Infinity; // TODO: 没有图片的时候整点说明文本或者干脆隐藏整个 frame

            float startX = Model.CoverSprite != null
                ? Model.CoverSprite.rect.x * imageFrameRect.rect.width / Model.ChartPackData.CropStartPosition.x
                : 0;
            float startY = Model.CoverSprite != null
                ? Model.CoverSprite.rect.y * imageFrameRect.rect.height / Model.ChartPackData.CropStartPosition.y
                : 0;
            startX = Mathf.Max(0, Mathf.Min(startX, imageFrameRect.rect.width));
            startY = Mathf.Max(0, Mathf.Min(startY, imageFrameRect.rect.height));
            coverCropAreaRect.anchoredPosition = new Vector2(startX, startY);

            float width = Model.CoverSprite != null
                ? Model.CoverSprite.rect.x * imageFrameRect.rect.width / Model.ChartPackData.CropWidth
                : 0;
            float height = width / 4;
            width = Mathf.Max(0, Mathf.Min(width, imageFrameRect.rect.width - startX));
            height = Mathf.Max(0, Mathf.Min(height, imageFrameRect.rect.height - startY));
            coverCropAreaRect.sizeDelta = new Vector2(width, height);

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
