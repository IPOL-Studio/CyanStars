using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游谱面item
    /// </summary>
    public class MapItem : BaseUIItem
    {
        public Image ImgCover;
        public TextMeshProUGUI TxtName;
        public Button BtnMap;

        public MapItemData Data { get; private set; }

        public override void OnCreate()
        {
            BtnMap.onClick.AddListener(() =>
            {
                MapSelectionPanel parent = GameRoot.UI.GetUIPanel<MapSelectionPanel>();
                parent.OnSelectMap(this);
            });
        }

        public override void OnGet()
        {
            ImgCover.sprite = null;
        }

        public override void OnRelease()
        {
            if (Data != null)
            {
                ReferencePool.Release(Data);
                Data = null;
            }
        }

        public void RefreshItem(MapItemData data)
        {
            Data = data;
            RefreshView();
        }

        public async void RefreshView()
        {
            TxtName.text = Data.MapManifest.Name;
            if (!string.IsNullOrEmpty(Data.MapManifest.ClipCoverFileName))
            {
                Sprite sprite = await GameRoot.Asset.LoadAssetAsync<Sprite>(Data.MapManifest.ClipCoverFileName,gameObject);
                ImgCover.sprite = sprite;
            }
            else
            {
                ImgCover.sprite = null;
            }
        }

        public void SetAlpha(float alpha)
        {
            // TODO
            ImgCover.color = new Color(ImgCover.color.r, ImgCover.color.g, ImgCover.color.b, alpha);
            TxtName.color = new Color(TxtName.color.r, TxtName.color.g, TxtName.color.b, alpha);
        }

    }
}
