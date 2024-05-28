using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.UI;
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


        private MusicGameModule musicGameModule;

        private List<BaseUIItem> mapItems = new List<BaseUIItem>();

        private MapSelectionPanelR owner;
        private CanvasGroup canvasGroup;

        private IPageElementAnimation[] animationElements;
        private Tween runningTween;

        public void OnInit(MapSelectionPanelR owner)
        {
            this.owner = owner;
            musicGameModule = GameRoot.GetDataModule<MusicGameModule>();
            canvasGroup = GetComponent<CanvasGroup>();

            animationElements = GetComponentsInChildren<IPageElementAnimation>(true);

            nextStepButton.onClick.AddListener(() =>
            {
                this.owner.ChangePage<StaffPage>();
            });

            // backButton.onClick.AddListener(() =>
            // {
            //     if (this.owner.IsActive<MapListPage>())
            //     {

            //     }
            // });
        }

        public async void OnEnter(MapSelectionPageChangeArgs args)
        {
            if (runningTween?.IsPlaying() ?? false)
            {
                runningTween.Kill(true);
            }

            gameObject.SetActive(true);

            if (mapItems.Count == 0)
            {
                circularMapList.ResetItems();
                await RefreshMusicList();
            }

            OnSelectMap(mapItems[this.owner.CurrentSelectedMap.Index] as MapItem);
            canvasGroup.alpha = 0;
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
            List<MapManifest> maps = musicGameModule.GetMaps();

            for (int i = 0; i < maps.Count; i++)
            {
                MapManifest map = maps[i];
                MapItem mapItem = await GameRoot.UI.AwaitGetUIItem<MapItem>(mapItemTemplate, circularMapList.transform);
                circularMapList.AddItem(mapItem);
                mapItems.Add(mapItem);
                mapItem.Index = i;

                MapItemData data = MapItemData.Create(i, map);
                mapItem.RefreshItem(data);
                mapItem.OnSelect += OnSelectMap;
            }

            mapItemTemplate.SetActive(false);
        }

        private void OnSelectMap(MapItem mapItem)
        {
            // 将选中的谱面移到圆环中央，即使当前已经选中也执行
            circularMapList.MoveToItemAt(mapItem.Index);

            if (this.owner.CurrentSelectedMap == mapItem.Data)
            {
                return;
            }

            this.owner.CurrentSelectedMap = mapItem.Data;

            Debug.Log("当前选中:" + mapItem.Data.MapManifest.Name);

            // 标题和Staff信息渐变动画
            mapTitleText.DOFade(0, 0.2f).OnComplete(() =>
            {
                mapTitleText.text = mapItem.Data.MapManifest.Name;
                mapTitleText.DOFade(1, 0.2f);
            });

            // 将原始 Staff 文本传递给 StarsGenerator 以进一步处理
            if (string.IsNullOrEmpty(mapItem.Data.MapManifest.StaffInfo) || string.IsNullOrWhiteSpace(mapItem.Data.MapManifest.StaffInfo))
            {
                Debug.LogWarning("没有设置 Staff 文本");
            }
            else
            {
                this.owner.StarController.ResetAllStaffGroup(mapItem.Data.MapManifest.StaffInfo);
            }
        }
    }
}
