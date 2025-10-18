#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class CoverCropFrame : BaseView, IDragHandler
    {
        [SerializeField]
        private RectTransform imageFrameRect = null!;

        public void OnDrag(PointerEventData eventData)
        {
            float x = eventData.delta.x / imageFrameRect.rect.width * 2;
            float y = eventData.delta.y / imageFrameRect.rect.height * 2;
            x = Mathf.Max(-1, Mathf.Min(x, 1));
            y = Mathf.Max(-1, Mathf.Min(y, 1));
            Vector2 deltaPositionRatio = new Vector2(x, y);
            Model.UpdateCoverCropByFrame(deltaPositionRatio);
        }
    }
}
