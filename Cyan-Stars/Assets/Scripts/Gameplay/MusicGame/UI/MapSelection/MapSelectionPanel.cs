using System;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MapSelectionUI/MapSelectionPanel.prefab")]
    public class MapSelectionPanel : BaseUIPanel
    {
        [SerializeField]
        private Toggle autoModeToggle;
        [SerializeField]
        private Button backButton;

        public StarController StarController;

        private Dictionary<Type, IMapSelectionPage> pageDict;
        private Stack<IMapSelectionPage> pageStack = new Stack<IMapSelectionPage>();

        private float pageRatio;
        private Tween starTween;

        public ChartPackItemData CurrentSelectedChartPack { get; set; }

        public void ChangePage<T>() where T : IMapSelectionPage
        {
            if (!pageDict.TryGetValue(typeof(T), out IMapSelectionPage page))
            {
                Debug.LogWarning("page can not be null");
                return;
            }

            var args = new MapSelectionPageChangeArgs()
            {
                FadeTime = 1.2f,
                AnimationEase = Ease.OutQuart
            };

            var currentPage = pageStack.Count > 0 ? pageStack.Peek() : null;

            if (starTween?.IsPlaying() ?? false)
                starTween.Kill(false);

            starTween = DOTween.To(() => pageRatio, x => pageRatio = x, pageRatio + 1, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnUpdate(() => StarController.OnUpdate(pageRatio))
                .OnComplete(() =>starTween = null);

            currentPage?.OnExit(args);

            currentPage = page;
            pageStack.Push(currentPage);
            currentPage.OnEnter(args);
        }

        public bool IsActive<T>() where T : IMapSelectionPage
        {
            return pageStack.Count > 0 && pageStack.Peek().GetType() == typeof(T);
        }

        protected override void OnCreate()
        {
            var musicGameModule = GameRoot.GetDataModule<MusicGameModule>();
            CurrentSelectedChartPack = ChartPackItemData.Create(musicGameModule.MapIndex, musicGameModule.GetChartPacks()[musicGameModule.MapIndex]);

            var pages = this.GetComponentsInChildren<IMapSelectionPage>(true);
            pageDict = new Dictionary<Type, IMapSelectionPage>();

            foreach (var page in pages)
            {
                page.OnInit(this);
                pageDict.Add(page.GetType(), page);
                ((Component)page).gameObject.SetActive(false);
            }

            autoModeToggle.onValueChanged.AddListener((isOn) =>
                GameRoot.GetDataModule<MusicGameModule>().IsAutoMode = isOn
            );

            backButton.onClick.AddListener(() =>
            {
                if (pageStack.Count > 1)
                {
                    BackToPrePage();
                }
                else if (pageStack.Count == 1)
                {
                    // TODO: 返回到上一个页面
                }
            });
        }

        public override void OnOpen()
        {
            pageRatio = 0;
            pageStack.Clear();
            ChangePage<MapListPage>();
            StarController.GenerateStars();
        }

        private void BackToPrePage()
        {
            var args = new MapSelectionPageChangeArgs()
            {
                FadeTime = 1.2f,
                AnimationEase = Ease.OutQuart
            };

            if (starTween?.IsPlaying() ?? false)
                starTween.Kill(false);

            starTween = DOTween.To(() => pageRatio, x => pageRatio = x, pageRatio - 1, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnUpdate(() => StarController.OnUpdate(pageRatio))
                .OnComplete(() =>starTween = null);

            pageStack.Pop().OnExit(args);
            pageStack.Peek().OnEnter(args);
        }
    }

    public interface IMapSelectionPage
    {
        void OnInit(MapSelectionPanel owner);
        void OnEnter(MapSelectionPageChangeArgs args);
        void OnExit(MapSelectionPageChangeArgs args);
    }

    public struct MapSelectionPageChangeArgs
    {
        public float FadeTime;
        public Ease AnimationEase;
    }
}
