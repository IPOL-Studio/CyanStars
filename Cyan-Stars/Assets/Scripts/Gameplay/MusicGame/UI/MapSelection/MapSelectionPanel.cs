using System;
using System.Collections.Generic;
using CyanStars.Chart;
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

        public MapItemData CurrentSelectedMap { get; set; }

        protected override void OnCreate()
        {
            // 进入选曲页时，如果没有选中谱包，自动选择第一个谱包
            // TODO: 在新建谱包进入制谱器时，此值可能为 null，后续考虑改为 bool 标记
            // TODO: 改为玩家上次选择的序号
            var chartModule = GameRoot.GetDataModule<ChartModule>();
            if (chartModule.SelectedChartPackIndex == null)
                chartModule.SelectChartPackData(0);
            CurrentSelectedMap =
                MapItemData.Create((int)chartModule.SelectedChartPackIndex!, chartModule.SelectedRuntimeChartPack!);

            // 查找、初始化、注册选曲页子页面组件
            var pages = this.GetComponentsInChildren<IMapSelectionPage>(true);
            pageDict = new Dictionary<Type, IMapSelectionPage>();
            foreach (var page in pages)
            {
                page.OnInit(this);
                pageDict.Add(page.GetType(), page);
                ((Component)page).gameObject.SetActive(false);
            }

            // 绑定各个组件回调
            autoModeToggle.onValueChanged.AddListener((isOn) =>
                GameRoot.GetDataModule<MusicGamePlayingDataModule>().IsAutoMode = isOn
            );
            backButton.onClick.AddListener(() =>
            {
                if (pageStack.Count > 1)
                {
                    BackToPrePage();
                }
                else if (pageStack.Count == 1)
                {
                    // TODO: 返回到主菜单
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

        public void ChangePage<T>() where T : IMapSelectionPage
        {
            if (!pageDict.TryGetValue(typeof(T), out IMapSelectionPage page))
            {
                Debug.LogWarning("page can not be null");
                return;
            }

            var args = new MapSelectionPageChangeArgs() { FadeTime = 1.2f, AnimationEase = Ease.OutQuart };

            var currentPage = pageStack.Count > 0 ? pageStack.Peek() : null;

            if (starTween?.IsPlaying() ?? false)
                starTween.Kill(false);

            starTween = DOTween.To(() => pageRatio, x => pageRatio = x, pageRatio + 1, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnUpdate(() => StarController.OnUpdate(pageRatio))
                .OnComplete(() => starTween = null);

            currentPage?.OnExit(args);

            currentPage = page;
            pageStack.Push(currentPage);
            currentPage.OnEnter(args);
        }

        private void BackToPrePage()
        {
            var args = new MapSelectionPageChangeArgs() { FadeTime = 1.2f, AnimationEase = Ease.OutQuart };

            if (starTween?.IsPlaying() ?? false)
                starTween.Kill(false);

            starTween = DOTween.To(() => pageRatio, x => pageRatio = x, pageRatio - 1, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnUpdate(() => StarController.OnUpdate(pageRatio))
                .OnComplete(() => starTween = null);

            pageStack.Pop().OnExit(args);
            pageStack.Peek().OnEnter(args);
        }

        public bool IsActive<T>() where T : IMapSelectionPage
        {
            return pageStack.Count > 0 && pageStack.Peek().GetType() == typeof(T);
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
