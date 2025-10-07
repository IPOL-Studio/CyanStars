#nullable enable

using System;
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

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);
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

            float x = localPoint.x / imageFrameRect.rect.width;
            float y = localPoint.y / imageFrameRect.rect.height;
            x = Mathf.Max(0, Mathf.Min(x, 1));
            y = Mathf.Max(0, Mathf.Min(y, 1));
            Vector2 pointPositionRatio = new Vector2(x, y);

            Model.UpdateCoverCropByHandles(type, pointPositionRatio);
        }
    }
}
