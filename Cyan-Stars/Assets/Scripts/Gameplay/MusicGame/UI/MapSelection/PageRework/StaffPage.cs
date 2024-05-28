using CyanStars.Framework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StaffPage : MonoBehaviour, IMapSelectionPage
    {
        [SerializeField]
        private Button startButton;

        private CanvasGroup canvasGroup;
        private Tween runningTween;

        public void OnInit(MapSelectionPanelR owner)
        {
            startButton.onClick.AddListener(() =>
            {
                GameRoot.GetDataModule<MusicGameModule>().MapIndex = owner.CurrentSelectedMap.Index;
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            });

            canvasGroup = GetComponent<CanvasGroup>();
        }

        public  void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            gameObject.SetActive(true);
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;

            runningTween = canvasGroup.DOFade(1, args.FadeTime)
                                      .SetEase(args.AnimationEase)
                                      .OnComplete(() => canvasGroup.interactable = true)
                                      .OnKill(() => runningTween = null);
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = false;

            runningTween = canvasGroup.DOFade(0, args.FadeTime)
                                      .SetEase(args.AnimationEase)
                                      .OnComplete(() => gameObject.SetActive(false))
                                      .OnKill(() => runningTween = null);
        }
    }
}
