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
        private RectTransform imageFrameRect = null!;

        [SerializeField]
        private Canvas mainCanvas = null!;


        private RectTransform? selfRect;

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

            ViewModel.CoverCropLeftBottomHandlerPercentPos
                .Subscribe(vector2 =>
                    {
                        selfRect.anchorMin = vector2;
                        selfRect.offsetMin = Vector2.zero;
                        selfRect.offsetMax = Vector2.zero;
                    }
                )
                .AddTo(this);
            ViewModel.CoverCropRightTopHandlerPercentPos
                .Subscribe(vector2 =>
                    {
                        selfRect.anchorMax = vector2;
                        selfRect.offsetMin = Vector2.zero;
                        selfRect.offsetMax = Vector2.zero;
                    }
                )
                .AddTo(this);
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
