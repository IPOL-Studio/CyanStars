#nullable enable

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面列表中的单个 item。
    /// 负责在父物体范围内横向移动指定的子物体。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ChartItem : MonoBehaviour
    {
        [Header("依赖组件")]
        [Tooltip("需要横向移动的子物体，其 RectTransform 轴心应为 (0.5, 0.5)")]
        [SerializeField]
        private RectTransform childRect = null!;

        /// <summary>
        /// 设置子物体的横向位置。
        /// </summary>
        /// <param name="normalizedX">归一化横坐标，范围 [0, 1]。
        /// 0 表示子物体左边缘与父物体左边缘贴合，1 表示子物体右边缘与父物体右边缘贴合。</param>
        public void SetXPos(float normalizedX)
        {
            float t = Mathf.Clamp01(normalizedX);

            RectTransform parentRect = (RectTransform)transform;
            float parentWidth = parentRect.rect.width;
            float childWidth = childRect.rect.width;

            float left = -parentRect.pivot.x * parentWidth;
            float right = (1f - parentRect.pivot.x) * parentWidth;

            float minX = left + childWidth * 0.5f;
            float maxX = right - childWidth * 0.5f;

            float targetX = minX + (maxX - minX) * t;

            Vector3 localPosition = childRect.localPosition;
            localPosition.x = targetX;
            childRect.localPosition = localPosition;
        }
    }
}
