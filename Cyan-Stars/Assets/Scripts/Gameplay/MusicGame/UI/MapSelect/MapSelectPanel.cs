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

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游谱面选择界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MapSelectUI/MapSelectPanel.prefab")]
    public class MapSelectPanel : BaseUIPanel
    {
        public Image ImgBg;
        public Transform MapListContentParent;
        public GameObject MapItemTemplate;
        public Button BtnStart;
        public Image ImgCover;
        public VideoPlayer VideoCover;
        public Toggle ToggleAutoMode;

        private MusicGameModule dataModule;
        private List<BaseUIItem> mapItems = new List<BaseUIItem>();
        private MapItem curSelectMapItem;

        protected override void OnCreate()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();

            ToggleAutoMode.onValueChanged.AddListener((isOn =>
            {
                //勾选了自动模式
                dataModule.IsAutoMode = isOn;
            }));

            BtnStart.onClick.AddListener(() =>
            {
                //切换到音游流程
                GameRoot.GetDataModule<MusicGameModule>().MapIndex = curSelectMapItem.Data.Index;
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            });
        }

        public override async void OnOpen()
        {
            ImgBg.sprite = null;
            ImgCover.sprite = null;
            VideoCover.clip = null;

            await RefreshMusicList();

            //默认选中上次选的谱面
            int selectedIndex = dataModule.MapIndex;
            OnSelectMap((MapItem)mapItems[selectedIndex]);

            ToggleAutoMode.isOn = dataModule.IsAutoMode;
        }

        public override void OnClose()
        {


            GameRoot.UI.ReleaseUIItems(mapItems);
            mapItems.Clear();
            curSelectMapItem = null;
        }

        /// <summary>
        /// 刷新谱面列表
        /// </summary>
        private async Task RefreshMusicList()
        {
            List<MapManifest> maps = dataModule.GetMaps();

            for (int i = 0; i < maps.Count; i++)
            {
                MapManifest map = maps[i];
                MapItem mapItem = await GameRoot.UI.AwaitGetUIItem<MapItem>(MapItemTemplate, MapListContentParent);
                mapItems.Add(mapItem);

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
            if (curSelectMapItem == mapItem)
            {
                return;
            }

            curSelectMapItem = mapItem;

            Debug.Log("当前选中:" + mapItem.Data.MapManifest.Name);

            if (string.IsNullOrEmpty(mapItem.Data.MapManifest.BackgroundFileName))
            {
                ImgBg.sprite = null;
            }
            else
            {
                Texture2D texture2D =
                    await GameRoot.Asset.AwaitLoadAsset<Texture2D>(mapItem.Data.MapManifest.BackgroundFileName,
                        gameObject);
                ImgBg.sprite = texture2D.ConvertToSprite();
            }


            if (string.IsNullOrEmpty(mapItem.Data.MapManifest.CoverFileName))
            {
                ImgCover.sprite = null;
                VideoCover.clip = null;
                ImgCover.gameObject.SetActive(true);
                VideoCover.gameObject.SetActive(false);
            }
            else
            {
                ImgCover.gameObject.SetActive(true);
                VideoCover.gameObject.SetActive(true);

                if (mapItem.Data.MapManifest.CoverFileName.EndsWith(".mp4"))
                {
                    VideoCover.clip =
                        await GameRoot.Asset.AwaitLoadAsset<VideoClip>(mapItem.Data.MapManifest.CoverFileName,
                            gameObject);
                    ImgCover.gameObject.SetActive(false);
                }
                else
                {
                    Texture2D texture2D =
                        await GameRoot.Asset.AwaitLoadAsset<Texture2D>(mapItem.Data.MapManifest.CoverFileName,
                            gameObject);
                    ImgCover.sprite = texture2D.ConvertToSprite();
                    VideoCover.gameObject.SetActive(false);
                }
            }
        }
    }
}
