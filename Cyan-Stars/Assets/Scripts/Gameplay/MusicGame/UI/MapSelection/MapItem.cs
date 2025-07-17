using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游谱面item
    /// </summary>
    public class MapItem : BaseUIItem
    {
        public Image ImgCover;

        public Image Mask;
        public TextMeshProUGUI TxtName;
        public Button BtnMap;

        public MapItemData Data { get; private set; }

        public int Index;

        // TODO: 此事件可能会导致内存泄漏或多次订阅，当前没有独立使用，因此没有问题。后续可能需要处理
        [SerializeField]
        private UnityEvent<MapItem> onSelect;

        public event UnityAction<MapItem> OnSelect
        {
            add => onSelect.AddListener(value);
            remove => onSelect.RemoveListener(value);
        }

        public override void OnCreate()
        {
            BtnMap.onClick.AddListener(Select);
        }

        public void Select()
        {
            // MapSelectionPanel parent = GameRoot.UI.GetUIPanel<MapSelectionPanel>();
            // parent.OnSelectMap(this);
            onSelect.Invoke(this);
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
            TxtName.text = Data.ChartPack.ChartPackData.Title;
            if (!string.IsNullOrEmpty(Data.ChartPack.ChartPackData.CoverFilePath))
            {
                // 从文件加载外部曲绘，并转为 MapItem 所需的 sprite 格式
                byte[] imageBytes = await GameRoot.Asset.LoadAssetAsync<byte[]>(
                    Data.ChartPack.ChartPackData.CroppedCoverFilePath, gameObject);
                Texture2D texture = new Texture2D(2,2);
                texture.LoadImage(imageBytes);

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                ImgCover.sprite = sprite;
            }
            else
            {
                ImgCover.sprite = null;
            }
        }

        public void SetAlpha(float alpha)
        {
            ImgCover.color = new Color(ImgCover.color.r, ImgCover.color.g, ImgCover.color.b, alpha);
            Mask.color = new Color(Mask.color.r, Mask.color.g, Mask.color.b, alpha);
            TxtName.color = new Color(TxtName.color.r, TxtName.color.g, TxtName.color.b, alpha);
        }
    }
}
