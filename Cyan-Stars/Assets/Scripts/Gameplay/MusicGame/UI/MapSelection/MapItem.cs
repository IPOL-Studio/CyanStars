#nullable enable

using System.Threading.Tasks;
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
            base.OnCreate();

            btnMap.onClick.AddListener(Select);
        }

        public override void OnRelease()
        {
            base.OnRelease();

            if (Data != null)
            {
                ReferencePool.Release(Data);
                Data = null;
            }

            handler?.Unload();
        }

        /// <summary>
        /// 外界在调用 GetUIItemAsync 后需要立刻调用此方法传入依赖
        /// </summary>
        /// <remarks>需要一些时间来加载曲绘</remarks>
        public async Task Init(MapItemData data)
        {
            Data = data;

            txtName.text = Data.RuntimeChartPack!.ChartPackData.Title;
            await LoadCoverTexture();
        }

        private async Task LoadCoverTexture()
        {
            if (!string.IsNullOrEmpty(Data!.RuntimeChartPack!.ChartPackData.CoverFilePath))
            {
                string coverFilePath =
                    PathUtil.Combine(Data.RuntimeChartPack.WorkspacePath, Data.RuntimeChartPack.ChartPackData.CoverFilePath);
                handler = await GameRoot.Asset.LoadAssetAsync<Texture2D>(coverFilePath);
                coverRawImage.texture = handler.Asset;

                float uvX = Data.RuntimeChartPack.ChartPackData.CropStartPositionPercent?.x ?? 0f;
                float uvY = Data.RuntimeChartPack.ChartPackData.CropStartPositionPercent?.y ?? 0f;
                float uvW = (Data.RuntimeChartPack.ChartPackData.CropHeightPercent ?? 0f) * handler.Asset.height * 4 / handler.Asset.width;
                float uvH = Data.RuntimeChartPack.ChartPackData.CropHeightPercent ?? 0f;
                coverRawImage.uvRect = new Rect(uvX, uvY, uvW, uvH);
            }
            else
            {
                coverRawImage.texture = null;
            }
        }

        public void Select()
        {
            onSelect.Invoke(this);
        }

        public void SetAlpha(float alpha)
        {
            coverRawImage.color = new Color(coverRawImage.color.r, coverRawImage.color.g, coverRawImage.color.b, alpha);
            mask.color = new Color(mask.color.r, mask.color.g, mask.color.b, alpha);
            txtName.color = new Color(txtName.color.r, txtName.color.g, txtName.color.b, alpha);
        }
    }
}
