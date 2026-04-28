#nullable enable

using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils
{
    /// <summary>
    /// 自定义 ScrollRect，用于分离鼠标拖拽和滚轮的控制
    /// </summary>
    public class CustomScrollRect : ScrollRect
    {
        /// <summary>
        /// 是否允许鼠标/触屏拖动
        /// </summary>
        public bool IsDragEnabled { get; set; } = true;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnEndDrag(eventData);
        }
    }
}
