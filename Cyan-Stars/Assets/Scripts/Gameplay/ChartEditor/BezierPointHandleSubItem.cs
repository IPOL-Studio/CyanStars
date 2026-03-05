#nullable enable

using CyanStars.Gameplay.ChartEditor.View;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor
{
    public enum BezierPointSubItemType
    {
        PosPoint,
        LeftControlPoint,
        RightControlPoint
    }

    /// <summary>
    /// 挂载到 BezierPointHandleItem 下的位置点和控制点上的脚本，用于向 BezierPointHandleItem 传递点击和拖拽事件
    /// </summary>
    public class BezierPointHandleSubItem : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private SpeedTemplateBezierPointHandleItemView bezierPointHandleItemView = null!;

        [SerializeField]
        private BezierPointSubItemType type;


        public void OnPointerClick(PointerEventData eventData)
        {
            bezierPointHandleItemView.OnSubObjectPointClick(eventData, type);
        }

        public void OnDrag(PointerEventData eventData)
        {
            bezierPointHandleItemView.OnSubObjectDrag(eventData, type);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            bezierPointHandleItemView.OnSubObjectBeginDrag(eventData, type);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            bezierPointHandleItemView.OnSubObjectEndDrag(eventData, type);
        }
    }
}
