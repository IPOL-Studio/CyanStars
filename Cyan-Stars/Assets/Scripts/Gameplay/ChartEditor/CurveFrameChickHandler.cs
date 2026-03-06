using CyanStars.Gameplay.ChartEditor.View;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor
{
    public class CurveFrameChickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private SpeedTemplateCurveFrameView parentView = null!;


        /// <summary>
        /// 在一个位置点上两次点击时，小于此间隔视为一次双击
        /// </summary>
        private const float DoubleClickDelaySecond = 0.5f;

        /// <summary>
        /// 上次点击时的 Time.unscaledTime
        /// </summary>
        /// <remarks>如果成功触发双击，将此值设为 0，避免三次点击触发两次双击判断的神秘逻辑</remarks>
        private float lastClickTime = 0;


        public void OnPointerClick(PointerEventData eventData)
        {
            // 单击时仅用于取消选中的贝塞尔点，多次触发 ok，无需在双击时过滤
            parentView.OnClick();

            // 双击交互逻辑
            if (Time.unscaledTime - lastClickTime <= DoubleClickDelaySecond)
            {
                // 触发双击
                lastClickTime = 0;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)transform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                );

                // 由于参与自动布局，轴心必须为 0.5,0.5，故手动调整 x 轴偏移量
                localPoint = new Vector2(localPoint.x + ((RectTransform)transform).rect.width / 2, localPoint.y);

                parentView.OnDoubleClick(localPoint);
            }
            else
            {
                lastClickTime = Time.unscaledTime;
            }
        }
    }
}
