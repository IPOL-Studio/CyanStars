#nullable enable

using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Utils;
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
        [SerializeField]
        private RawImage coverRawImage = null!;

        [SerializeField]
        private Image mask = null!;

        [SerializeField]
        private TextMeshProUGUI txtName = null!;

        [SerializeField]
        private Button btnMap = null!;

        // TODO: 此事件可能会导致内存泄漏或多次订阅，当前没有独立使用，因此没有问题。后续可能需要处理
        [SerializeField]
        private UnityEvent<MapItem> onSelect = null!;


        public MapItemData? Data { get; private set; }

        private AssetHandler<Texture2D>? handler;


        public event UnityAction<MapItem> OnSelect
        {
            add => onSelect.AddListener(value);
            remove => onSelect.RemoveListener(value);
        }

        public override void OnCreate()
        {
            btnMap.onClick.AddListener(Select);
        }

        public void Select()
        {
            // MapSelectionPanel parent = GameRoot.UI.GetUIPanel<MapSelectionPanel>();
            // parent.OnSelectMap(this);
            onSelect.Invoke(this);
        }

        public override void OnGet()
        {
        }

        public override void OnRelease()
        {
            if (Data != null)
            {
                ReferencePool.Release(Data);
                Data = null;
            }

            handler?.Unload();
        }

        public void Init(MapItemData data)
        {
            Data = data;

            txtName.text = Data.RuntimeChartPack!.ChartPackData.Title;
            LoadCoverTexture();
        }

        private async void LoadCoverTexture()
        {
            if (!string.IsNullOrEmpty(Data!.RuntimeChartPack!.ChartPackData.CoverFilePath))
            {
                string coverFilePath =
                    PathUtil.Combine(Data.RuntimeChartPack.WorkspacePath, Data.RuntimeChartPack.ChartPackData.CoverFilePath);
                handler = await GameRoot.Asset.LoadAssetAsync<Texture2D>(coverFilePath);
                coverRawImage.texture = handler.Asset;
            }
            else
            {
                coverRawImage.texture = null;
            }
        }

        public void SetAlpha(float alpha)
        {
            coverRawImage.color = new Color(coverRawImage.color.r, coverRawImage.color.g, coverRawImage.color.b, alpha);
            mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, alpha);
            txtName.color = new Color(txtName.color.r, txtName.color.g, txtName.color.b, alpha);
        }
    }
}
