using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class PageElementTranslation : MonoBehaviour, IPageElementAnimation
    {
        private Vector2 defaultPos;
        private RectTransform rectTransform;

        public Vector2 RelativeExitTargetPos;

        private Tween runningTween;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            defaultPos = rectTransform.anchoredPosition;
        }

        public void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.anchoredPosition = defaultPos + RelativeExitTargetPos;
            runningTween = rectTransform.DOAnchorPos(defaultPos, args.FadeTime).SetEase(args.AnimationEase);
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.anchoredPosition = defaultPos;
            runningTween = rectTransform.DOAnchorPos(defaultPos + RelativeExitTargetPos, args.FadeTime).SetEase(args.AnimationEase);
        }
    }
}
