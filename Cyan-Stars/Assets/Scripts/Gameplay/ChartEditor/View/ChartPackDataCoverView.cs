#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 谱包弹窗内关于曲绘裁剪部分的 View
    /// </summary>
    public class ChartPackDataCoverView : BaseView<ChartPackDataCoverViewModel>
    {
        [SerializeField]
        private RectTransform imageAreaFrameRect = null!;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter = null!;

        [SerializeField]
        private RawImage baseRawImage = null!;

        [SerializeField]
        private RawImage highlightRawImage = null!;

        [SerializeField]
        private RectMask2D mask = null!;

        [SerializeField]
        private RectTransform cropAreaRect = null!;

        [SerializeField]
        private Button importCoverButton = null!;


        public override void Bind(ChartPackDataCoverViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.ImageFrameAspectRatio
                .Subscribe(value => aspectRatioFitter.aspectRatio = value)
                .AddTo(this);

            ViewModel.CoverSprite
                .Subscribe(sprite =>
                    {
                        baseRawImage.texture = sprite?.texture;
                        highlightRawImage.texture = sprite?.texture;
                    }
                )
                .AddTo(this);

            ViewModel.CropLeftBottomPercentPos
                .Subscribe(_ =>
                    {
                        OnCropDataChanged();
                    }
                )
                .AddTo(this);
            ViewModel.CropRightTopPercentPos
                .Subscribe(_ =>
                    {
                        OnCropDataChanged();
                    }
                )
                .AddTo(this);

            importCoverButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.OpenCoverBrowser())
                .AddTo(this);
        }

        private void OnCropDataChanged()
        {
            // 强制立即刷新 imageAreaFrameRect 的布局，以确保计算正确
            LayoutRebuilder.ForceRebuildLayoutImmediate(imageAreaFrameRect);
            Canvas.ForceUpdateCanvases();

            var leftBottomPercentPos = ViewModel.CropLeftBottomPercentPos.CurrentValue;
            var rightTopPercentPos = ViewModel.CropRightTopPercentPos.CurrentValue;

            Rect maskRect = mask.rectTransform.rect;

            mask.padding = new Vector4(
                leftBottomPercentPos.x * maskRect.width,
                leftBottomPercentPos.y * maskRect.height,
                (1 - rightTopPercentPos.x) * maskRect.width,
                (1 - rightTopPercentPos.y) * maskRect.height
            );

            cropAreaRect.anchorMin = leftBottomPercentPos;
            cropAreaRect.anchorMax = rightTopPercentPos;
            cropAreaRect.offsetMin = Vector2.zero;
            cropAreaRect.offsetMax = Vector2.zero;
        }
    }
}
