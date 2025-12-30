#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataCoverCropHandlerView : BaseView<ChartPackDataViewModel>, IDragHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private CoverCropHandlerType type;

        [SerializeField]
        private RectTransform imageFrameRect = null!;

        private RectTransform selfRect = null!;


        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            selfRect = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 点击时将当前 Handle 置于最上层，防止被其他 Handle 遮挡导致无法拖拽
            selfRect.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ViewModel == null || ViewModel.LoadedCoverSprite.CurrentValue == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    imageFrameRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            float normalizedX = (localPoint.x - imageFrameRect.rect.x) / imageFrameRect.rect.width;
            float normalizedY = (localPoint.y - imageFrameRect.rect.y) / imageFrameRect.rect.height;

            ViewModel.SetCoverCropHandlerPos(type, new Vector2(normalizedX, normalizedY));
        }

        protected override void OnDestroy()
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ViewModel?.OnCoverCropDragBegin();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            ViewModel?.OnCoverCropDragEnd();
        }
    }
}
