#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 谱包弹窗内曲绘裁剪的四个顶点的 View，每个顶点持有一个 View。
    /// </summary>
    public class ChartPackDataCoverCropHandlerView : BaseView<ChartPackDataCoverViewModel>, IDragHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private CoverCropHandlerType type;

        [SerializeField]
        private RectTransform imageFrameRect = null!;

        private RectTransform selfRect = null!;


        public override void Bind(ChartPackDataCoverViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            selfRect = GetComponent<RectTransform>();

            // handler 无需监听 ViewModel 变化，所有的变化由其父物体 frame 自动更新，handler 由 Unity 布局系统自动更新位置到 frame 四角
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // 点击时将当前 Handle 置于最上层，防止被其他 Handle 遮挡导致无法拖拽
            selfRect.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ViewModel?.CoverSprite.CurrentValue == null)
                return;

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

            ViewModel.OnHandlerDragging(type, new Vector2(normalizedX, normalizedY));
        }

        public void OnBeginDrag(PointerEventData _)
        {
            ViewModel?.RecordCropData();
        }

        public void OnEndDrag(PointerEventData _)
        {
            ViewModel?.CommitCropData();
        }
    }
}
