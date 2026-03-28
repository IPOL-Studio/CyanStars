#nullable enable

using CyanStars.EditorExtension;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    public class Star : MonoBehaviour
    {
        [SerializeField]
        private Image starImage = null!;

        [SerializeField]
        private StaffLabel staffLabel = null!;


        /// <summary>
        /// 这个星星在第几组显示？
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 位移灵敏度，每往后翻一页，星星在 x 方向上移动多少距离（一般是负数，代表向左）
        /// </summary>
        public Vector3 PosParallax { get; set; }

        /// <summary>
        /// 星星的不透明度
        /// </summary>
        public float Alpha { get; set; }

        /// <summary>
        /// 该组件在可用页时的位置比例（以左下为原点，相对于PanelWidth）
        /// </summary>
        public Vector3 PosRatio;


        public void SetStarScale(float size)
        {
            starImage.transform.localScale = Vector3.one * size;
        }

        public void SetStaffLabelText(string dutyStr, string nameStr)
        {
            staffLabel.SetText(dutyStr, nameStr);
        }

        public void SetRender(float targetAlpha, float gradientTime)
        {
            staffLabel.SetRender(targetAlpha, gradientTime);
        }

        public void SetStaffLabelActive(bool isActive)
        {
            staffLabel.gameObject.SetActive(isActive);
        }

        public void UpdateFromScreenRatio(RectTransform root, float ratio)
        {
            ((RectTransform)transform).localPosition = CalculatePosFormScreenRatio(root, ratio);
        }

        /// <summary>
        /// 计算在指定页面上的坐标
        /// </summary>
        /// <param name="root">父物体的 RectTransform</param>
        /// <param name="ratio">横向位移乘积比例（位于第几页），会受到 star 自身的横向位移灵敏度影响</param>
        /// <returns>star 在第 ratio 页中位于 root 左下角的坐标位置</returns>
        public Vector3 CalculatePosFormScreenRatio(RectTransform root, float ratio)
        {
            var panelSize = root.rect.size;
            float x = (PosRatio.x + ratio * PosParallax.x) * panelSize.x;
            float y = PosRatio.y * panelSize.y;

            return new Vector3(x, y, PosRatio.z);
        }

        /// <summary>
        /// 返回 StaffLabel RectTransform 的大小
        /// </summary>
        /// <returns></returns>
        public Vector2 GetStaffLabelSize()
        {
            return ((RectTransform)staffLabel.transform).rect.size;
        }
    }
}
