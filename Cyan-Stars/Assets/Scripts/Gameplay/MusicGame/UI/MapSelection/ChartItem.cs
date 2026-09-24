#nullable enable

using System;
using CyanStars.Chart;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [Tooltip("需要横向移动的子物体，其 RectTransform 轴心应为 (0.5, 0.5)")]
        [SerializeField]
        private RectTransform childRect = null!;

        [SerializeField]
        private Image chartImage = null!;

        [SerializeField]
        private TMP_Text chartText = null!;

        [SerializeField]
        private Button button = null!;


        [SerializeField]
        private Sprite kuiXingUnselectedSprite = null!;

        [SerializeField]
        private Sprite kuiXingSelectedSprite = null!;

        [SerializeField]
        private Sprite qiMingUnselectedSprite = null!;

        [SerializeField]
        private Sprite qiMingSelectedSprite = null!;

        [SerializeField]
        private Sprite tianShuUnselectedSprite = null!;

        [SerializeField]
        private Sprite tianShuSelectedSprite = null!;

        [SerializeField]
        private Sprite wuYinUnselectedSprite = null!;

        [SerializeField]
        private Sprite wuYinSelectedSprite = null!;


        private ChartDifficulty difficultyCache;


        /// <summary>
        /// 当前 item 对应的谱面在谱包 ChartMetaDatas 列表中的下标。
        /// </summary>
        public int ChartIndex { get; private set; }

        /// <summary>
        /// button 被点击时触发。
        /// </summary>
        public event Action<ChartItem>? OnClicked;


        private void OnEnable() => button.onClick.AddListener(OnClick);

        private void OnDisable() => button.onClick.RemoveListener(OnClick);


        /// <summary>
        /// 由 UI 的 Button 点击事件调用，通知外界该 item 被点击。
        /// </summary>
        private void OnClick() => OnClicked?.Invoke(this);

        /// <summary>
        /// 在实例化/取回宿主 go 马上后调用此方法来构造
        /// </summary>
        /// <param name="textString">要显示的文本</param>
        /// <param name="difficulty">谱面难度</param>
        /// <param name="chartIndex">谱面在谱包 ChartMetaDatas 列表中的下标。</param>
        /// <param name="isSelected">是否被选中</param>
        public void Init(string textString, ChartDifficulty difficulty, int chartIndex, bool isSelected)
        {
            chartText.text = textString;
            difficultyCache = difficulty;
            ChartIndex = chartIndex;
            SetSprite(isSelected);
        }

        /// <summary>
        /// 设置选中/取消选中状态
        /// </summary>
        /// <param name="isSelected"></param>
        public void SetSprite(bool isSelected)
        {
            chartImage.sprite = difficultyCache switch
            {
                ChartDifficulty.KuiXing => isSelected ? kuiXingSelectedSprite : kuiXingUnselectedSprite,
                ChartDifficulty.QiMing => isSelected ? qiMingSelectedSprite : qiMingUnselectedSprite,
                ChartDifficulty.TianShu => isSelected ? tianShuSelectedSprite : tianShuUnselectedSprite,
                ChartDifficulty.WuYin => isSelected ? wuYinSelectedSprite : wuYinUnselectedSprite,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

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
