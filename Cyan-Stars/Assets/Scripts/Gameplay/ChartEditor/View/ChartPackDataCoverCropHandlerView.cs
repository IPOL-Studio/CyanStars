#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataCoverCropHandlerView : BaseView<ChartPackDataViewModel>, IDragHandler, IPointerDownHandler
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

            ReadOnlyReactiveProperty<Vector2> targetProperty = type switch
            {
                CoverCropHandlerType.LeftTop => targetViewModel.CoverCropLeftTopHandlerPercentPos,
                CoverCropHandlerType.LeftBottom => targetViewModel.CoverCropLeftBottomHandlerPercentPos,
                CoverCropHandlerType.RightTop => targetViewModel.CoverCropRightTopHandlerPercentPos,
                CoverCropHandlerType.RightBottom => targetViewModel.CoverCropRightBottomHandlerPercentPos,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            targetProperty
                .Subscribe(UpdateHandlePosition)
                .AddTo(gameObject);
        }

        private void UpdateHandlePosition(Vector2 percentPos)
        {
            selfRect.anchorMin = percentPos;
            selfRect.anchorMax = percentPos;
            selfRect.anchoredPosition = Vector2.zero;
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

            // 将屏幕坐标转换为相对于 imageFrameRect 的局部坐标
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    imageFrameRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            float normalizedY = (localPoint.y - imageFrameRect.rect.y) / imageFrameRect.rect.height;
            normalizedY = Mathf.Clamp01(normalizedY);
            ViewModel.SetCoverCropHandlerPos(type, normalizedY);
        }

        protected override void OnDestroy()
        {
        }
    }
}
