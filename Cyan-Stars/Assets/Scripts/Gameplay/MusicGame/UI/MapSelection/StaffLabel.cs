#nullable enable

using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StaffLabel : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup = null!;

        [SerializeField]
        private TMP_Text dutyText = null!;

        [SerializeField]
        private TMP_Text nameText = null!;

        private Tween? tween;


        private void Awake()
        {
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 设置职务和 staff 文本，并立刻刷新自动布局
        /// </summary>
        /// <param name="dutyStr">职务文本</param>
        /// <param name="nameStr">staff 名字</param>
        public void SetText(string dutyStr, string nameStr)
        {
            dutyText.text = dutyStr;
            nameText.text = nameStr;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        /// <summary>
        /// 用 DoTween 来更新整个 Label 的 alpha
        /// </summary>
        /// <param name="targetAlpha">目标 alpha [0,1]</param>
        /// <param name="gradientTime">目标缓动时间</param>
        public void SetRender(float targetAlpha, float gradientTime)
        {
            if (tween?.IsPlaying() ?? false)
                tween.Kill(false);

            tween = canvasGroup.DOFade(targetAlpha, gradientTime)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => tween = null);
        }
    }
}
