#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.MarkdownRenderer.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MapListPage : MonoBehaviour, IMapSelectionPage
    {
        [SerializeField]
        private ChartPackCircularLayout chartPackCircularLayout = null!;

        [SerializeField]
        private Button nextStepButton = null!;

        [SerializeField]
        private TextMeshProUGUI mapTitleText = null!;


        public event Action? OnNextStepRequested;

        private CanvasGroup canvasGroup = null!;
        private StarController starController = null!;

        private IPageElementAnimation[] animationElements = null!;
        private Tween? runningTween;

        private ChartModule chartModule = null!;
        private readonly HashSet<string> StaffNames = new();

        private int? selectedChartPackIndex;


        public void OnInit(StarController controller)
        {
            starController = controller;
            canvasGroup = GetComponent<CanvasGroup>();

            animationElements = GetComponentsInChildren<IPageElementAnimation>(true);

            chartModule = GameRoot.GetDataModule<ChartModule>();

            mapTitleText.text = ""; // 防止编辑器内的示例标题参与首次打开 UI 时的淡出动画
            nextStepButton.onClick.AddListener(() => OnNextStepRequested?.Invoke());
            chartPackCircularLayout.OnChartPackItemClicked += OnChartPackItemClicked;
        }

        private void OnDestroy()
        {
            if (chartPackCircularLayout != null)
            {
                chartPackCircularLayout.OnChartPackItemClicked -= OnChartPackItemClicked;
            }
        }

        public async void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            selectedChartPackIndex = null;
            await RefreshMusicList();

            if (chartModule.RuntimeChartPacks.Count > 0)
            {
                SelectChartPack(chartModule.SelectedChartPackIndex ?? 0);
            }

            runningTween = canvasGroup.DOFade(1, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnKill(() => runningTween = null);

            foreach (var elem in animationElements)
            {
                elem.OnEnter(args);
            }
        }

        public void OnExit(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            canvasGroup.alpha = 1;

            foreach (var elem in animationElements)
            {
                elem.OnExit(args);
            }

            runningTween = canvasGroup.DOFade(0, args.FadeTime)
                .SetEase(args.AnimationEase)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                })
                .OnKill(() => runningTween = null);
        }

        /// <summary>
        /// 刷新谱包列表
        /// </summary>
        private async Task RefreshMusicList()
        {
            IReadOnlyList<RuntimeChartPack> chartPacks = chartModule.RuntimeChartPacks;
            MapItemData[] chartPackItemData = new MapItemData[chartPacks.Count];
            for (int i = 0; i < chartPacks.Count; i++)
            {
                chartPackItemData[i] = MapItemData.Create(i, chartPacks[i]);
            }

            await chartPackCircularLayout.RebuildItemsAsync(chartPackItemData);
        }

        private void OnChartPackItemClicked(int index)
        {
            SelectChartPack(index);
        }

        private void SelectChartPack(int index)
        {
            if (index < 0 || index >= chartModule.RuntimeChartPacks.Count)
                return;

            // 同一个谱包重复点击无需重复刷新标题和 Staff
            if (selectedChartPackIndex == index)
                return;

            chartModule.SelectChartPackData(index);
            selectedChartPackIndex = index;

            RuntimeChartPack runtimeChartPack = chartModule.RuntimeChartPacks[index];
            UpdateMapTitle(runtimeChartPack);
            UpdateStaff(runtimeChartPack);
        }

        /// <summary>
        /// 更新标题和 Staff 信息渐变动画
        /// </summary>
        private void UpdateMapTitle(RuntimeChartPack runtimeChartPack)
        {
            string title = runtimeChartPack.ChartPackData.Title;
            mapTitleText.DOFade(0, 0.2f).OnComplete(() =>
            {
                mapTitleText.text = title;
                mapTitleText.DOFade(1, 0.2f);
            });
        }

        private void UpdateStaff(RuntimeChartPack runtimeChartPack)
        {
            if (chartModule.SelectedMusicVersionIndex == null)
            {
                Debug.LogWarning("没有设置音乐版本");
                return;
            }

            StaffNames.Clear();

            foreach (AtInfo atInfo in MarkdownUtils.CollectCysAtInfo(runtimeChartPack.ChartPackData.ChartPackInfo))
            {
                StaffNames.Add(atInfo.Content);
            }

            starController.ResetAllStaffGroup(StaffNames);
        }
    }
}
