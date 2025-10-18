#nullable enable

using CyanStars.GamePlay.ChartEditor.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public enum CoverCropHandleType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class CoverCropHandle : BaseView, IDragHandler
    {
        [SerializeField]
        private CoverCropHandleType type;

        [SerializeField]
        private RectTransform imageFrameRect = null!;

        private RectTransform selfRect = null!;


        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            selfRect = gameObject.GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    imageFrameRect,
                    eventData.position,
                    null,
                    out Vector2 localPoint))
            {
                return;
            }

            // 在 UI 中将元素置顶，这样即使与其他 Handle 重叠也能正确拖动
            selfRect.SetAsLastSibling();

            float x = localPoint.x / imageFrameRect.rect.width;
            float y = localPoint.y / imageFrameRect.rect.height;
            x = Mathf.Max(0, Mathf.Min(x, 1));
            y = Mathf.Max(0, Mathf.Min(y, 1));
            Vector2 pointPositionRatio = new Vector2(x, y);

            Model.UpdateCoverCropByHandles(type, pointPositionRatio);
        }
    }
}
