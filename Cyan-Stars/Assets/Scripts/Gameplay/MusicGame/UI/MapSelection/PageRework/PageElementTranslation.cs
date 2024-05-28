using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class PageElementTranslation : MonoBehaviour, IPageElementAnimation
    {
        private Vector3 defaultPos;
        private RectTransform rectTransform;

        public Vector3 RelativeExitTargetPos;

        private Tween runningTween;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            defaultPos = rectTransform.localPosition;
        }

        public void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.localPosition = defaultPos + RelativeExitTargetPos;
            runningTween = rectTransform.DOLocalMove(defaultPos, args.FadeTime).SetEase(args.AnimationEase);
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
                runningTween.Kill();

            rectTransform.localPosition = defaultPos;
            runningTween = rectTransform.DOLocalMove(defaultPos + RelativeExitTargetPos, args.FadeTime).SetEase(args.AnimationEase);
        }
    }
}
