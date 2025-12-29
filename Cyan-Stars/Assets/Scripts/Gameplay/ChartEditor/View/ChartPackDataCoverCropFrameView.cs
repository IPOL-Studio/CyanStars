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
    public class ChartPackDataCoverCropFrameView : BaseView<ChartPackDataViewModel>, IDragHandler
    {
        [Header("References")]
        [SerializeField]
        private RectTransform imageFrameRect = null!; // 底图的 RectTransform，用于计算比例

        [SerializeField]
        private Canvas mainCanvas = null!; // 主 Canvas，用于计算缩放后的拖拽量

        private RectTransform selfRect = null!;

        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
            selfRect = GetComponent<RectTransform>();

            targetViewModel.CoverCropAnchorMin
                .Subscribe(min =>
                {
                    selfRect.anchorMin = min;
                    selfRect.offsetMin = Vector2.zero;
                })
                .AddTo(gameObject);

            targetViewModel.CoverCropAnchorMax
                .Subscribe(max =>
                {
                    selfRect.anchorMax = max;
                    selfRect.offsetMax = Vector2.zero;
                })
                .AddTo(gameObject);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ViewModel == null || ViewModel.LoadedCoverSprite.CurrentValue == null)
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

            ViewModel.MoveCoverCrop(deltaPositionRatio);
        }

        protected override void OnDestroy()
        {
        }
    }
}
