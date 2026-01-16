using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 挂载在 BeatLine Prefab 上的组件，用于快速设置样式
    /// </summary>
    public class BeatLineItem : MonoBehaviour
    {
        [SerializeField] private Image lineImage;
        [SerializeField] private GameObject textGroup;
        [SerializeField] private TextMeshProUGUI indexText;

        private static readonly Color ColorInteger = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ColorHalf = new Color(1f, 0.7f, 0.4f, 0.8f);
        private static readonly Color ColorQuarter = new Color(0.4f, 0.7f, 1f, 0.7f);
        private static readonly Color ColorOther = new Color(0.6f, 1f, 0.6f, 0.6f);

        public RectTransform RectTransform => (RectTransform)transform;

        public void SetVisuals(int beatIndex, int accuracy)
        {
            // 计算当前线在这一拍中的 Index（0 到 accuracy-1）
            int mod = beatIndex % accuracy;

            if (mod == 0)
            {
                // 整数拍
                lineImage.color = ColorInteger;
                textGroup.SetActive(true);
                indexText.text = (beatIndex / accuracy).ToString();
            }
            else
            {
                textGroup.SetActive(false);

                // 1/2 拍
                if (accuracy % 2 == 0 && mod == accuracy / 2)
                {
                    lineImage.color = ColorHalf;
                }
                // 1/4 或 3/4 拍
                else if (accuracy % 4 == 0 && (mod == accuracy / 4 || mod == accuracy / 4 * 3))
                {
                    lineImage.color = ColorQuarter;
                }
                // 其他
                else
                {
                    lineImage.color = ColorOther;
                }
            }
        }
    }
}
