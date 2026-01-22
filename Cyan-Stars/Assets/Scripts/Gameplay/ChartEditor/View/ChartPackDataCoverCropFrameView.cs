#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 负责显示裁剪框区域并处理整体拖拽（平移）
    /// </summary>
    public class ChartPackDataCoverCropFrameView : BaseView<ChartPackDataCoverViewModel>, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField]
        private RectTransform imageFrameRect = null!;

        [SerializeField]
        private Canvas mainCanvas = null!;

        private RectTransform? selfRect;


        public override void Bind(ChartPackDataCoverViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            selfRect = this.transform as RectTransform;

            ViewModel.CropLeftBottomPercentPos
                .Subscribe(_ => RefreshPos())
                .AddTo(this);
            ViewModel.CropRightTopPercentPos
                .Subscribe(_ => RefreshPos())
                .AddTo(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ViewModel?.CoverSprite.CurrentValue == null)
            {
                return;
            }

            if (imageFrameRect.rect.width == 0 || imageFrameRect.rect.height == 0)
            {
                return;
            }

            float xRatioDelta = eventData.delta.x / imageFrameRect.rect.width / mainCanvas.scaleFactor;
            float yRatioDelta = eventData.delta.y / imageFrameRect.rect.height / mainCanvas.scaleFactor;

            Vector2 deltaPositionRatio = new Vector2(xRatioDelta, yRatioDelta);

            ViewModel.OnFrameDragging(deltaPositionRatio);
        }

        private void RefreshPos()
        {
            selfRect.anchorMin = ViewModel.CropLeftBottomPercentPos.CurrentValue;
            selfRect.anchorMax = ViewModel.CropRightTopPercentPos.CurrentValue;
            selfRect.offsetMin = Vector2.zero;
            selfRect.offsetMax = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData _)
        {
            ViewModel?.RecordCropData();
        }

        public void OnEndDrag(PointerEventData _)
        {
            ViewModel?.CommitCropData();
        }

        protected override void OnDestroy()
        {
        }
    }
}
