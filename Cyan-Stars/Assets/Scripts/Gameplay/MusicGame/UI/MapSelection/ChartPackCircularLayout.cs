#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱包列表的纵向 ScrollView 布局。
    /// item 的高度与 Content 高度由既有的自动布局（VerticalLayoutGroup + ContentSizeFitter）负责；
    /// 滚动 Content 时，会通过 <see cref="ChartPackItem"/> 调整每个 item 的横向位置，形成环形滚动效果。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ChartPackCircularLayout : MonoBehaviour
    {
        [Header("依赖组件")]
        [SerializeField]
        private ScrollRect scrollRect = null!;

        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private GameObject chartPackItemTemplate = null!;


        private bool isLoading = false;
        private readonly List<Task> TasksCache = new();
        private readonly Dictionary<BaseUIItem, int> ChartPackItemToIndexCache = new();
        private readonly Dictionary<BaseUIItem, AssetHandler<Texture2D?>> ChartPackItemToCoverHandlerCache = new();
        private readonly Dictionary<BaseUIItem, MapItemData> ChartPackItemToDataCache = new();

        // 缓存 RectTransform 大小状态
        private Vector2 lastRectSize;
        private bool isDirty = false;


        /// <summary>
        /// 谱包 item 被点击时触发，参数为谱包下标。
        /// </summary>
        public event Action<int>? OnChartPackItemClicked;


        private struct LoadContext
        {
            public MapItemData Data;
            public BaseUIItem? CompletedItem;
            public Task<BaseUIItem>? ItemTask;
            public Task<AssetHandler<Texture2D?>?>? CoverTask;
        }


        private void Start()
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void Update()
        {
            // 玩家屏幕高度变化后，在帧末统一重新计算布局，避免在尺寸变化回调中直接改布局
            if (isDirty)
                ApplyLayout(false);
        }

        private void OnDestroy()
        {
            if (scrollRect != null)
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        private void OnRectTransformDimensionsChange()
        {
            // 当 RectTransform 大小改变时，记录其大小，并设为脏数据
            Vector2 currentSize = ((RectTransform)transform).rect.size;
            if (!isDirty && currentSize != lastRectSize)
                isDirty = true;
            lastRectSize = currentSize;
        }


        public async Task RebuildItemsAsync(MapItemData[] datas)
        {
            if (isLoading)
                throw new NotImplementedException("正在加载，暂不支持中途取消。");

            isLoading = true;
            TasksCache.Clear();

            LoadContext[] contexts = new LoadContext[datas.Length];

            try
            {
                ReleaseChartPackItems();

                for (int i = 0; i < datas.Length; i++)
                {
                    MapItemData data = datas[i];
                    BaseUIItem? completedItem = null;
                    Task<BaseUIItem>? itemTask = null;

                    // 尝试同步加载 BaseUIItem：
                    // GetUIItemAsync 返回 ValueTask，对象池中有可复用对象时该任务已同步完成，此时直接读取 Result；
                    // 否则缓存 AsTask 后的 Task，稍后统一并发等待。
                    ValueTask<BaseUIItem> itemValueTask =
                        GameRoot.UI.GetUIItemAsync<BaseUIItem>(chartPackItemTemplate, contentRect);

                    if (itemValueTask.IsCompletedSuccessfully)
                    {
                        completedItem = itemValueTask.Result;
                    }
                    else
                    {
                        // 收集未能同步加载的 BaseUIItem
                        itemTask = itemValueTask.AsTask();
                        TasksCache.Add(itemTask);
                    }

                    // 创建 Cover Assets 加载任务
                    Task<AssetHandler<Texture2D?>?> coverTask = LoadCoverAssetAsync(data);
                    TasksCache.Add(coverTask);

                    contexts[i] = new LoadContext
                    {
                        Data = data, CompletedItem = completedItem, ItemTask = itemTask, CoverTask = coverTask
                    };
                }

                // 并发任务
                await Task.WhenAll(TasksCache);

                // 初始化各 BaseUIItem
                foreach (var context in contexts)
                {
                    BaseUIItem item = context.CompletedItem ?? context.ItemTask!.Result;
                    AssetHandler<Texture2D?>? coverHandler = context.CoverTask!.Result;

                    InitializeChartPackItem(context.Data, item, coverHandler);
                    CacheChartPackItem(context.Data, item, coverHandler);
                }

                // 重新生成 item 后回到顶部，并应用布局
                contentRect.anchoredPosition = Vector2.zero;
                ApplyLayout(true);
            }
            catch
            {
                // 构建失败时回收本次已获取的 item，并卸载已加载的 Cover Assets
                ReleaseAcquiredResources(contexts);
                ChartPackItemToIndexCache.Clear();
                ChartPackItemToCoverHandlerCache.Clear();
                ChartPackItemToDataCache.Clear();
                throw;
            }
            finally
            {
                TasksCache.Clear();
                isLoading = false;
            }
        }

        /// <summary>
        /// 创建单个谱包封面的加载任务。
        /// </summary>
        private static async Task<AssetHandler<Texture2D?>?> LoadCoverAssetAsync(MapItemData data)
        {
            RuntimeChartPack? runtimeChartPack = data.RuntimeChartPack;
            if (runtimeChartPack == null)
                return null;

            string? coverFilePath = runtimeChartPack.ChartPackData.CoverFilePath;
            if (string.IsNullOrEmpty(coverFilePath))
                return null;

            string fullCoverFilePath = PathUtil.Combine(runtimeChartPack.WorkspacePath, coverFilePath);
            return await GameRoot.Asset.LoadAssetAsync<Texture2D?>(fullCoverFilePath);
        }

        /// <summary>
        /// 初始化单个 BaseUIItem。
        /// </summary>
        private void InitializeChartPackItem(
            MapItemData data,
            BaseUIItem item,
            AssetHandler<Texture2D?>? coverHandler)
        {
            if (item is not ChartPackItem chartPackItem)
            {
                Debug.LogWarning(
                    $"谱包 item 模板上未挂载 {nameof(ChartPackItem)} 组件，无法初始化：{item.name}",
                    this);
                return;
            }

            // 1:4 裁剪曲绘
            Texture2D coverTexture = coverHandler?.Asset ?? new Texture2D(1, 1);
            float x = data.RuntimeChartPack?.ChartPackData.CropStartPositionPercent?.x ?? 0;
            float y = data.RuntimeChartPack?.ChartPackData.CropStartPositionPercent?.y ?? 0;
            float h = data.RuntimeChartPack?.ChartPackData.CropHeightPercent ?? 1;
            float hPixel = coverTexture.height * h;
            float wPixel = 4 * hPixel;
            float w = wPixel / coverTexture.width;
            Rect coverRect = new Rect(x, y, w, h);

            string title = data.RuntimeChartPack?.ChartPackData.Title ?? string.Empty;
            chartPackItem.Init(
                coverTexture,
                coverRect,
                title,
                () => NotifyChartPackItemClicked(data.Index)
            );
        }

        /// <summary>
        /// 将构建完成的 item 及其 Cover 句柄写入缓存，供后续重建时统一回收。
        /// </summary>
        private void CacheChartPackItem(
            MapItemData data,
            BaseUIItem item,
            AssetHandler<Texture2D?>? coverHandler)
        {
            ChartPackItemToIndexCache[item] = data.Index;
            ChartPackItemToDataCache[item] = data;

            if (coverHandler != null)
            {
                ChartPackItemToCoverHandlerCache[item] = coverHandler;
            }
        }

        /// <summary>
        /// 归还当前缓存的 item，并卸载其 Cover 句柄。
        /// </summary>
        private void ReleaseChartPackItems()
        {
            List<BaseUIItem> items = ChartPackItemToIndexCache.Keys.ToList();
            foreach (BaseUIItem item in items)
            {
                // 先禁用，确保本帧的自动布局不会再计算旧 item，随后由 UI 对象池回收
                item.gameObject.SetActive(false);
            }

            GameRoot.UI.ReleaseUIItems(items);

            foreach (BaseUIItem item in ChartPackItemToDataCache.Keys)
            {
                if (ChartPackItemToDataCache.TryGetValue(item, out MapItemData? data))
                    ReferencePool.Release(data);
            }

            foreach (var t in ChartPackItemToCoverHandlerCache.Values)
            {
                t.Unload();
            }

            ChartPackItemToCoverHandlerCache.Clear();
            ChartPackItemToIndexCache.Clear();
            ChartPackItemToDataCache.Clear();
        }

        /// <summary>
        /// 回收一次失败构建中已经获取但尚未入缓存的对象与资源。
        /// </summary>
        private static void ReleaseAcquiredResources(LoadContext[] contexts)
        {
            foreach (var context in contexts)
            {
                if (context.CompletedItem != null)
                    GameRoot.UI.ReleaseUIItem(context.CompletedItem);
                else if (context.ItemTask != null && context.ItemTask.IsCompletedSuccessfully && context.ItemTask.Result != null)
                    GameRoot.UI.ReleaseUIItem(context.ItemTask.Result);

                if (context.CoverTask != null && context.CoverTask.IsCompletedSuccessfully && context.CoverTask.Result != null)
                    context.CoverTask.Result.Unload();

                if (context.Data != null)
                    ReferencePool.Release(context.Data);
            }
        }

        private void NotifyChartPackItemClicked(int index)
        {
            OnChartPackItemClicked?.Invoke(index);
        }

        /// <summary>
        /// 应用当前布局：让 Content 按既有自动布局重新计算尺寸，并刷新环形横向位置。
        /// </summary>
        /// <param name="resetScrollPosition">是否把 Content 滚动位置重置回顶部。</param>
        private void ApplyLayout(bool resetScrollPosition)
        {
            isDirty = false;
            lastRectSize = ((RectTransform)transform).rect.size;

            if (resetScrollPosition)
                contentRect.anchoredPosition = Vector2.zero;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            Canvas.ForceUpdateCanvases();

            UpdateChartItemHorizontalLayout();
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            UpdateChartItemHorizontalLayout();
        }

        private void UpdateChartItemHorizontalLayout()
        {
            if (ChartPackItemToIndexCache.Count == 0)
                return;

            RectTransform viewportRect = scrollRect.viewport != null
                ? scrollRect.viewport
                : (RectTransform)scrollRect.transform;

            foreach (BaseUIItem item in ChartPackItemToIndexCache.Keys)
            {
                if (item is not ChartPackItem chartPackItem)
                    continue;

                RectTransform itemRect = (RectTransform)chartPackItem.transform;
                CircularLayoutHelper.SetItemXPos(itemRect, viewportRect, chartPackItem.SubItemWidth);
            }
        }
    }

}
