using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PageElementFade : MonoBehaviour, IPageElementAnimation
    {
        private CanvasGroup canvasGroup;
        public float FadeDuration;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.ignoreParentGroups = true;
            canvasGroup.alpha = 0f;
        }

        public void OnEnter(MapSelectionPageChangeArgs args)
        {
            canvasGroup.DOFade(1f, FadeDuration).SetEase(args.AnimationEase);
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            canvasGroup.DOFade(0f, FadeDuration).SetEase(args.AnimationEase);
        }
    }
}
