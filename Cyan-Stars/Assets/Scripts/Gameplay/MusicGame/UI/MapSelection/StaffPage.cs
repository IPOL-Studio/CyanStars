using System.Collections;
using CyanStars.Framework;
using CyanStars.Chart;
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

        [SerializeField]
        private float staffGroupKeepTime = 3f;

        private float lastStaffGroupKeepTime = -1;

        private WaitForSeconds staffCarouseInterval;

        private CanvasGroup canvasGroup;
        private Tween runningTween;

        private MapSelectionPanel owner;

        private Coroutine staffCarouseCor;


        public void OnInit(MapSelectionPanel owner)
        {
            this.owner = owner;

            startButton.onClick.AddListener(() =>
            {
                ChartModule module = GameRoot.GetDataModule<ChartModule>();
                module.SelectedChartPackIndex = owner.CurrentSelectedMap.Index;
                module.Difficulty = ChartDifficulty.KuiXing; // TODO: 从 UI 传入难度参数
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            });

            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnEnter(MapSelectionPageChangeArgs args)
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

            staffCarouseCor = StartCoroutine(CarouselStaffStarsCor());
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            if (staffCarouseCor != null)
            {
                StopCoroutine(staffCarouseCor);
                owner.StarController.ResetShowingGroup();
                staffCarouseCor = null;
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = false;

            runningTween = canvasGroup.DOFade(0, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnComplete(() => gameObject.SetActive(false))
                .OnKill(() => runningTween = null);
        }

        private IEnumerator CarouselStaffStarsCor()
        {
            while (true)
            {
                owner.StarController.ShowNextStaffGroup();

                if (staffGroupKeepTime <= 0)
                    staffGroupKeepTime = 3f;

                if (lastStaffGroupKeepTime != staffGroupKeepTime || staffCarouseInterval == null)
                {
                    lastStaffGroupKeepTime = staffGroupKeepTime;
                    staffCarouseInterval = new WaitForSeconds(staffGroupKeepTime);
                }

                yield return staffCarouseInterval;
            }
        }
    }
}
