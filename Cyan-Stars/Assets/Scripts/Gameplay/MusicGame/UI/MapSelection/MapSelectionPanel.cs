using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.UI;
using CyanStars.Framework.Utils;
using CyanStars.Gameplay.Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游谱面选择界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MapSelectionUI/MapSelectionPanel.prefab")]
    public class MapSelectionPanel : BaseUIPanel
    {
        [Header("MapItem模板对象")]
        public GameObject MapItemTemplate;

        [Header("选曲圆环控件")]
        public CircularLayout CircularMapList;

        [Header("开始按钮")]
        public Button StartButton;

        [Header("自动模式开关")]
        public Toggle ToggleAutoMode;

        [Header("谱面标题")]
        public TextMeshProUGUI TxtMapTitle;

        [Header("Staff信息")]
        public TextMeshProUGUI StaffInfoDisplay;

        [Header("PageController 组件")]
        public PageController PageController;

        [Header("StarsGenerator 组件")]
        public StarsGenerator Generator;

        /// <summary>
        /// 音游数据模块
        /// </summary>
        private MusicGameModule musicGameDataModule;

        /// <summary>
        /// 谱面item列表
        /// </summary>
        private readonly List<BaseUIItem> MapItems = new List<BaseUIItem>();

        /// <summary>
        /// 当前被选中的谱面item
        /// </summary>
        private MapItem curSelectedMapItem;

        protected override void OnCreate()
        {
            musicGameDataModule = GameRoot.GetDataModule<MusicGameModule>();

            ToggleAutoMode.onValueChanged.AddListener((isOn =>
            {
                //勾选了自动模式
                musicGameDataModule.IsAutoMode = isOn;
            }));

            StartButton.onClick.AddListener(() =>
            {
                //切换到音游流程
                GameRoot.GetDataModule<MusicGameModule>().MapIndex = curSelectedMapItem.Data.Index;
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            });
        }

        public override async void OnOpen()
        {
            CircularMapList.ResetItems();

            await RefreshMusicList();

            Generator.GenerateStars();

            PageController.OnOpen();


            //默认选中上次选的谱面
            int selectedIndex = musicGameDataModule.MapIndex;
            OnSelectMap((MapItem)MapItems[selectedIndex]);

            ToggleAutoMode.isOn = musicGameDataModule.IsAutoMode;


        }

        public override void OnClose()
        {
            GameRoot.UI.ReleaseUIItems(MapItems);
            MapItems.Clear();
            curSelectedMapItem = null;
        }

        /// <summary>
        /// 刷新谱面列表
        /// </summary>
        private async Task RefreshMusicList()
        {
            List<MapManifest> maps = musicGameDataModule.GetMaps();

            for (int i = 0; i < maps.Count; i++)
            {
                MapManifest map = maps[i];
                MapItem mapItem = await GameRoot.UI.AwaitGetUIItem<MapItem>(MapItemTemplate, CircularMapList.transform);
                CircularMapList.AddItem(mapItem);
                MapItems.Add(mapItem);
                mapItem.Index = i;

                MapItemData data = MapItemData.Create(i, map);
                mapItem.RefreshItem(data);
                mapItem.OnSelect += OnSelectMap;
            }

            MapItemTemplate.SetActive(false);
        }

        /// <summary>
        /// 选中谱面Item
        /// </summary>
        public void OnSelectMap(MapItem mapItem)
        {
            // 将选中的谱面移到圆环中央，即使当前已经选中也执行
            CircularMapList.MoveToItemAt(mapItem.Index);

            if (curSelectedMapItem == mapItem)
            {
                return;
            }

            curSelectedMapItem = mapItem;

            Debug.Log("当前选中:" + mapItem.Data.MapManifest.Name);

            // 标题和Staff信息渐变动画
            TxtMapTitle.DOFade(0, 0.2f).OnComplete(
                () =>
                {
                    TxtMapTitle.text = mapItem.Data.MapManifest.Name;
                    TxtMapTitle.DOFade(1, 0.2f);
                }
            );
            //StaffInfoDisplay.DOFade(0, 0.2f).OnComplete(
            //    () =>
            //    {
            //        StaffInfoDisplay.text = mapItem.Data.MapManifest.StaffInfo;
            //        StaffInfoDisplay.DOFade(1, 0.2f);
            //    }
            //);

            // 将原始 Staff 文本传递给 StarsGenerator 以进一步处理
            if (string.IsNullOrEmpty(mapItem.Data.MapManifest.StaffInfo) || string.IsNullOrWhiteSpace(mapItem.Data.MapManifest.StaffInfo))
            {
                Debug.LogWarning("没有设置 Staff 文本");
            }
            else
            {
                Generator.ResetAllStaffGroup(mapItem.Data.MapManifest.StaffInfo);
            }
        }
    }
}
