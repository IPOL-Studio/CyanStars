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
        public Button BtnStart;

        [Header("自动模式开关")]
        public Toggle ToggleAutoMode;

        [Header("谱面标题")]
        public TextMeshProUGUI TxtMapTitle;
        
        [Header("谱面曲绘图片")]
        public Image ImgCover;

        [Header("谱面曲绘视频")]
        public VideoPlayer VideoCover;

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

            BtnStart.onClick.AddListener(() =>
            {
                //切换到音游流程
                GameRoot.GetDataModule<MusicGameModule>().MapIndex = curSelectedMapItem.Data.Index;
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            });

            BtnStart.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }

        public override async void OnOpen()
        {
            // ImgBg.sprite = null;
            ImgCover.sprite = null;
            VideoCover.clip = null;
            CircularMapList.Reset();

            await RefreshMusicList();

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

                MapItemData data = MapItemData.Create(i, map);
                mapItem.RefreshItem(data);
            }

            MapItemTemplate.SetActive(false);
        }

        /// <summary>
        /// 选中谱面Item
        /// </summary>
        public async void OnSelectMap(MapItem mapItem)
        {
            if (curSelectedMapItem == mapItem)
            {
                return;
            }

            curSelectedMapItem = mapItem;

            Debug.Log("当前选中:" + mapItem.Data.MapManifest.Name);

            TxtMapTitle.DOFade(0, 0.2f).OnComplete(
                () =>
                {
                    TxtMapTitle.text = mapItem.Data.MapManifest.Name;
                    TxtMapTitle.DOFade(1, 0.2f);
                }
            );

            if (string.IsNullOrEmpty(mapItem.Data.MapManifest.CoverFileName))
            {
                ImgCover.sprite = null;
                VideoCover.clip = null;
                ImgCover.gameObject.SetActive(false);
                VideoCover.gameObject.SetActive(false);
            }
            else
            {
                if (mapItem.Data.MapManifest.CoverFileName.EndsWith(".mp4"))
                {
                    VideoClip clip = 
                        await GameRoot.Asset.LoadAssetAsync<VideoClip>(mapItem.Data.MapManifest.CoverFileName,
                            gameObject);
                    VideoCover.clip = clip;

                    RawImage videoRawImage = VideoCover.GetComponent<RawImage>();
                    videoRawImage.DOFade(1, 0.2f).SetEase(Ease.InSine);
                    ImgCover.DOFade(0, 0.3f).SetEase(Ease.InSine).OnComplete(()=>{ImgCover.gameObject.SetActive(false);});
                    VideoCover.gameObject.SetActive(true);
                }
                else
                {
                    Sprite sprite =
                        await GameRoot.Asset.LoadAssetAsync<Sprite>(mapItem.Data.MapManifest.CoverFileName, gameObject);
                    ImgCover.sprite = sprite;

                    RawImage videoRawImage = VideoCover.GetComponent<RawImage>();
                    ImgCover.DOFade(1, 0.2f).SetEase(Ease.InSine);
                    videoRawImage.DOFade(0, 0.3f).SetEase(Ease.InSine).OnComplete(()=>{VideoCover.gameObject.SetActive(false);});
                    ImgCover.gameObject.SetActive(true);
                }
                
            }
        }
    }
}