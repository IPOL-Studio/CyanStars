using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class PageElementTranslation : MonoBehaviour, IPageElementAnimation
    {
        [SerializeField]
        private Vector2 relativeExitTargetPos;

        private Tween runningTween;
        private RectTransform rectTransform;
        private Vector2 defaultPos;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            defaultPos = rectTransform.anchoredPosition;
        }

        public void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.anchoredPosition = defaultPos + relativeExitTargetPos;
            runningTween = rectTransform.DOAnchorPos(defaultPos, args.FadeTime).SetEase(args.AnimationEase);
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.anchoredPosition = defaultPos;
            runningTween = rectTransform.DOAnchorPos(defaultPos + relativeExitTargetPos, args.FadeTime).SetEase(args.AnimationEase);
        }
    }
}
