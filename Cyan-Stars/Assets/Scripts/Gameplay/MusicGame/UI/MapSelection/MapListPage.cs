using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Chart;
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
        private GameObject mapItemTemplate;

        [SerializeField]
        private CircularLayout circularMapList;

        [SerializeField]
        private Button nextStepButton;

        [SerializeField]
        private TextMeshProUGUI mapTitleText;


        private MapSelectionPanel owner;
        private CanvasGroup canvasGroup;

        private IPageElementAnimation[] animationElements;
        private Tween runningTween;

        private MusicGamePlayingDataModule musicGamePlayingDataModule;
        private ChartModule chartModule;
        private List<BaseUIItem> mapItems;


        public void OnInit(MapSelectionPanel owner)
        {
            this.owner = owner;
            canvasGroup = GetComponent<CanvasGroup>();

            animationElements = GetComponentsInChildren<IPageElementAnimation>(true);

            musicGamePlayingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();
            chartModule = GameRoot.GetDataModule<ChartModule>();
            mapItems = new List<BaseUIItem>();

            mapTitleText.text = ""; // 防止编辑器内的示例标题参与首次打开 UI 时的淡出动画
            nextStepButton.onClick.AddListener(() =>
            {
                this.owner.ChangePage<StaffPage>();
            });
        }

        public async void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            canvasGroup.alpha = 0;
            gameObject.SetActive(true);

            if (mapItems.Count == 0)
            {
                circularMapList.ResetItems();
                await RefreshMusicList();
            }

            OnSelectMap(mapItems[this.owner.CurrentSelectedMap.Index] as MapItem);
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
                .OnComplete(() => gameObject.SetActive(false))
                .OnKill(() => runningTween = null);
        }

        /// <summary>
        /// 刷新谱面列表
        /// </summary>
        private async Task RefreshMusicList()
        {
            IReadOnlyList<RuntimeChartPack> chartPacks = chartModule.RuntimeChartPacks;

            for (int i = 0; i < chartPacks.Count; i++)
            {
                RuntimeChartPack runtimeChartPack = chartPacks[i];
                MapItemData data = MapItemData.Create(i, runtimeChartPack);
                MapItem mapItem = await GameRoot.UI.GetUIItemAsync<MapItem>(mapItemTemplate, circularMapList.transform);
                await mapItem.Init(data);
                circularMapList.AddItem(mapItem);
                mapItems.Add(mapItem);
                mapItem.OnSelect += OnSelectMap;
            }
        }

        private void OnSelectMap(MapItem mapItem)
        {
            // 在 DataModule 中选中谱包
            if (mapItem.Data == null)
                throw new NullReferenceException(nameof(mapItem.Data));
            chartModule.SelectChartPackData(mapItem.Data.Index);

            // 将选中的谱面移到圆环中央，即使当前已经选中也执行
            circularMapList.MoveToItemAt(mapItem.Data!.Index);

            if (owner.CurrentSelectedMap == mapItem.Data)
            {
                return;
            }

            owner.CurrentSelectedMap = mapItem.Data;

            Debug.Log("当前选中:" + mapItem.Data.RuntimeChartPack!.ChartPackData.Title);

            // 标题和Staff信息渐变动画
            mapTitleText.DOFade(0, 0.2f).OnComplete(() =>
            {
                mapTitleText.text = mapItem.Data.RuntimeChartPack.ChartPackData.Title;
                mapTitleText.DOFade(1, 0.2f);
            });

            // 将原始 Staff 文本传递给 StarsGenerator 以进一步处理
            if (chartModule.SelectedMusicVersionIndex == null)
            {
                Debug.LogWarning("没有设置音乐版本");
                return;
            }

            Dictionary<string, List<string>> staffs = mapItem.Data.RuntimeChartPack.ChartPackData
                .MusicVersionDatas[(int)chartModule.SelectedMusicVersionIndex].Staffs;
            if (staffs.Count == 0)
            {
                Debug.LogWarning("没有设置 Staff 文本");
            }
            else
            {
                owner.StarController.ResetAllStaffGroup(mapItem.Data.RuntimeChartPack.ChartPackData
                    .MusicVersionDatas[(int)chartModule.SelectedMusicVersionIndex].Staffs);
            }
        }
    }
}
