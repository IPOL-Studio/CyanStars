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
    public class ChartPackCircularLayoutRefactor : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect scrollRect = null!;

        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private GameObject chartPackItemTemplate = null!;


        private bool isLoading = false;
        private readonly List<Task> TasksCache = new();
        private readonly Dictionary<BaseUIItem, int> ChartPackItemToIndexCache = new();
        private readonly Dictionary<BaseUIItem, AssetHandler<Sprite?>> ChartPackItemToCoverHandlerCache = new();


        /// <summary>
        /// 谱包 item 被点击时触发，参数为谱包下标。
        /// </summary>
        public event Action<int>? OnChartPackItemClicked;


        private struct LoadContext
        {
            public MapItemData Data;
            public BaseUIItem? CompletedItem;
            public Task<BaseUIItem>? ItemTask;
            public Task<AssetHandler<Sprite?>?>? CoverTask;
        }

        public async Task RebuildItemsAsync(MapItemData[] datas)
        {
            if (isLoading)
                throw new NotImplementedException("正在加载，暂不支持中途取消。");

            isLoading = true;
            TasksCache.Clear();

            // 预分配数组，避免中间集合的堆内存分配与冗余循环
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
                    Task<AssetHandler<Sprite?>?> coverTask = LoadCoverAssetAsync(data);
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
                    AssetHandler<Sprite?>? coverHandler = context.CoverTask!.Result;

                    InitializeChartPackItem(context.Data, item, coverHandler);
                    CacheChartPackItem(context.Data, item, coverHandler);
                }
            }
            catch
            {
                // 构建失败时回收本次已获取的 item，并卸载已加载的 Cover Assets
                ReleaseAcquiredResources(contexts);
                ChartPackItemToIndexCache.Clear();
                ChartPackItemToCoverHandlerCache.Clear();
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
        private static async Task<AssetHandler<Sprite?>?> LoadCoverAssetAsync(MapItemData data)
        {
            RuntimeChartPack? runtimeChartPack = data.RuntimeChartPack;
            if (runtimeChartPack == null)
                return null;

            string? coverFilePath = runtimeChartPack.ChartPackData.CoverFilePath;
            if (string.IsNullOrEmpty(coverFilePath))
                return null;

            string fullCoverFilePath = PathUtil.Combine(runtimeChartPack.WorkspacePath, coverFilePath);
            return await GameRoot.Asset.LoadAssetAsync<Sprite?>(fullCoverFilePath);
        }

        /// <summary>
        /// 初始化单个 BaseUIItem。
        /// </summary>
        private void InitializeChartPackItem(
            MapItemData data,
            BaseUIItem item,
            AssetHandler<Sprite?>? coverHandler)
        {
            if (item is not ChartPackItem chartPackItem)
            {
                Debug.LogWarning(
                    $"谱包 item 模板上未挂载 {nameof(ChartPackItem)} 组件，无法初始化：{item.name}",
                    this);
                return;
            }

            string title = data.RuntimeChartPack?.ChartPackData.Title ?? string.Empty;
            chartPackItem.Init(
                coverHandler?.Asset!,
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
            AssetHandler<Sprite?>? coverHandler)
        {
            ChartPackItemToIndexCache[item] = data.Index;

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
            GameRoot.UI.ReleaseUIItems(ChartPackItemToIndexCache.Keys.ToList());

            foreach (var t in ChartPackItemToCoverHandlerCache.Values)
            {
                t.Unload();
            }

            ChartPackItemToCoverHandlerCache.Clear();
            ChartPackItemToIndexCache.Clear();
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
            }
        }

        private void NotifyChartPackItemClicked(int index)
        {
            OnChartPackItemClicked?.Invoke(index);
        }
    }
}
