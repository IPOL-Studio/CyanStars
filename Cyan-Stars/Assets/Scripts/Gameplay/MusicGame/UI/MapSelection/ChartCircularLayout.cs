#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面列表的纵向 ScrollView 布局。
    /// 以 4 个 item 恰好铺满玩家 Canvas 高度作为基准，结合 Canvas 高度与 item 高度/间距高度比反推 item 高度和间距；
    /// item 不足 4 个时向上对齐（缺失的 item 表现为末尾空缺），item 多于 4 个时继续向下追加并撑高 Content。
    /// 滚动 Content 时，会通过 <see cref="ChartItem"/> 调整每个 item 内子物体的横向位置，
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ChartCircularLayout : MonoBehaviour
    {
        [Header("依赖组件")]
        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private VerticalLayoutGroup contentVerticalLayoutGroup = null!;

        [Tooltip("承载 Content 的 ScrollRect，用于监听滚动并更新弧形横坐标")]
        [SerializeField]
        private ScrollRect scrollRect = null!;

        [Tooltip("谱包轮盘，复用其椭圆参数计算谱面 item 的横向位置")]
        [SerializeField]
        private ChartPackCircularLayout circularLayout = null!;


        [Header("配置参数")]
        [Tooltip("上边距（px）")]
        [SerializeField]
        private float topMargin = 100f;

        [Tooltip("下边距（px）")]
        [SerializeField]
        private float bottomMargin = 100f;

        [Tooltip("item 高度与 item 间距高度之比")]
        [SerializeField, Min(0.01f)]
        private float itemHeightToSpacingHeightRatio = 2f;

        [Tooltip("Item 预制体")]
        [SerializeField]
        private GameObject itemPrefab = null!;

        /// <summary>
        /// 计算 item 高度与间距时作为基准的 item 数量。
        /// </summary>
        private const int BaseItemCount = 4;

        private ChartModule? chartModule;
        private ChartModule ChartModule => chartModule ??= GameRoot.GetDataModule<ChartModule>();

        private readonly GameObjectPoolManager GameObjectPool = GameRoot.GameObjectPool;
        private readonly Dictionary<ChartMetaData, ChartItem> MetaDataToChartItemDict = new();

        // 缓存 RectTransform 大小状态
        private Vector2 lastRectSize;
        private bool isDirty = false;

        private CancellationTokenSource? cts;

        /// <summary>
        /// 当前正在构建/显示的谱包，用于判断谱面选中事件是否仍属于当前列表。
        /// </summary>
        private RuntimeChartPack? currentRuntimeChartPack;


        private void Start()
        {
            cts = new();
            _ = BuildLayoutAsync(ChartModule.SelectedRuntimeChartPack, cts.Token);

            ChartModule.OnSelectedChartPackChanged += RefreshChartLayoutAwait;
            ChartModule.OnSelectedChartChanged += RefreshChartSelection;
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
            scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            ChartModule.OnSelectedChartChanged -= RefreshChartSelection;
            ChartModule.OnSelectedChartPackChanged -= RefreshChartLayoutAwait;
        }


        private void OnRectTransformDimensionsChange()
        {
            // 当 RectTransform 大小改变时，记录其大小，并设为脏数据
            Vector2 currentSize = ((RectTransform)transform).rect.size;
            if (!isDirty && currentSize != lastRectSize)
                isDirty = true;
            lastRectSize = currentSize;
        }

        private async void RefreshChartLayoutAwait(RuntimeChartPack? runtimeChartPack)
        {
            ReleaseLayout();
            cts = new();
            await BuildLayoutAsync(runtimeChartPack, cts.Token);
        }

        private void ReleaseLayout()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            foreach (ChartItem chartItem in MetaDataToChartItemDict.Values)
            {
                chartItem.OnClicked -= OnChartItemClicked;

                // 先禁用，确保本帧的自动布局不会再计算旧 item，随后延迟销毁
                chartItem.gameObject.SetActive(false);
                GameObjectPool.ReleaseGameObject(itemPrefab, chartItem.gameObject);
            }

            MetaDataToChartItemDict.Clear();
        }

        /// <summary>
        /// Chart item 被点击后，把点击的谱面预选到 ChartModule。
        /// </summary>
        private void OnChartItemClicked(ChartItem chartItem)
        {
            ChartModule.PreSelectChartData(chartItem.ChartIndex);
        }

        /// <summary>
        /// 根据 ChartModule 的谱面选中下标刷新各 item 的选中状态。
        /// </summary>
        /// <param name="selectedChartIndex">ChartModule 中选中的谱面下标，为 null 时取消选中。</param>
        private void RefreshChartSelection(int? selectedChartIndex)
        {
            // 谱包切换时会先触发谱面选中事件、再触发谱包切换事件。
            // 此时当前列表属于旧谱包，跳过刷新，等待随后按新谱包重建布局。
            if (currentRuntimeChartPack != ChartModule.SelectedRuntimeChartPack)
                return;

            foreach (ChartItem chartItem in MetaDataToChartItemDict.Values)
            {
                chartItem.SetSprite(chartItem.ChartIndex == selectedChartIndex);
            }
        }

        /// <summary>
        /// 从对象池构建 Layout
        /// </summary>
        private async Task BuildLayoutAsync(RuntimeChartPack? runtimeChartPack, CancellationToken cancellationToken = default)
        {
            currentRuntimeChartPack = runtimeChartPack;

            if (runtimeChartPack == null)
                return;

            // 筛选出非空难度，实例化 go，以元组形式填充到列表
            // 采用列表而非字典以保持顺序
            // 采用局部变量 pendingTasks 牺牲少量 GC 性能来避免异步竟态
            // 保留原始下标，用于对齐 ChartModule.SelectedChartIndex
            var pendingTasks = new List<(int chartIndex, ChartMetaData metaData, Task<GameObject> task)>();
            for (int chartIndex = 0; chartIndex < runtimeChartPack.ChartPackData.ChartMetaDatas.Count; chartIndex++)
            {
                var chartMetaData = runtimeChartPack.ChartPackData.ChartMetaDatas[chartIndex];
                if (chartMetaData.Difficulty != null)
                {
                    var task = GameObjectPool.GetGameObjectAsync(itemPrefab, contentRect, cancellationToken);
                    pendingTasks.Add((chartIndex, chartMetaData, task));
                }
            }

            try
            {
                await Task.WhenAll(pendingTasks.Select(x => x.task));
            }
            catch (OperationCanceledException)
            {
#if UNITY_EDITOR
                Debug.Log($"{nameof(ChartCircularLayout)}.{nameof(BuildLayoutAsync)}() 在创建时被释放，操作已取消。");
#endif
                foreach (var item in pendingTasks)
                {
                    if (item.task.Status != TaskStatus.RanToCompletion || item.task.Result == null)
                        continue;
                    item.task.Result.SetActive(false);
                    GameObjectPool.ReleaseGameObject(itemPrefab, item.task.Result);
                }
                return;
            }

            MetaDataToChartItemDict.Clear();

            foreach (var item in pendingTasks)
            {
                var go = item.task.Result;
                var chartItem = go.GetComponent<ChartItem>();
                ChartDifficulty difficulty = (ChartDifficulty)item.metaData.Difficulty!;

                string text;
                if (string.IsNullOrEmpty(item.metaData.OverrideDifficultyText))
                {
                    var difficultyText = difficulty switch
                    {
                        ChartDifficulty.KuiXing => "窥星",
                        ChartDifficulty.QiMing => "启明",
                        ChartDifficulty.TianShu => "天枢",
                        ChartDifficulty.WuYin => "无垠",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    var level = item.metaData.Level;
                    text = $"{difficultyText} Lv.{level}";
                }
                else
                {
                    text = item.metaData.OverrideDifficultyText;
                }

                chartItem.Init(text, difficulty, item.chartIndex, ChartModule.SelectedChartIndex == item.chartIndex);
                chartItem.OnClicked += OnChartItemClicked;

                MetaDataToChartItemDict[item.metaData] = chartItem;
            }

            // 重新生成 item 后回到顶部，并应用布局
            contentRect.anchoredPosition = Vector2.zero;
            ApplyLayout(true);
        }

        /// <summary>
        /// 应用当前布局：计算 item 高度与间距，设置自动布局参数，
        /// 并根据 item 数量设置 Content 高度。
        /// </summary>
        /// <param name="resetScrollPosition">是否把 Content 滚动位置重置回顶部。</param>
        private void ApplyLayout(bool resetScrollPosition)
        {
            // Content 的上下边距、item 间距、对齐方式由 Unity 自动布局接管
            // Content 总高度、各 item 高度由代码接管

            isDirty = false;
            lastRectSize = ((RectTransform)transform).rect.size;

            int itemCount = MetaDataToChartItemDict.Count;

            CalculateHeight(out float itemHeight, out float itemGapHeight);

            // 上下边距、item 间距、顶部对齐由 VerticalLayoutGroup 接管
            contentVerticalLayoutGroup.padding = new RectOffset(
                0,
                0,
                Mathf.RoundToInt(topMargin),
                Mathf.RoundToInt(bottomMargin)
            );
            contentVerticalLayoutGroup.spacing = itemGapHeight;
            contentVerticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
            contentVerticalLayoutGroup.childControlHeight = false;
            contentVerticalLayoutGroup.childForceExpandHeight = false;

            // 各 item 高度由代码接管
            foreach (ChartItem chartItem in MetaDataToChartItemDict.Values)
            {
                RectTransform itemRect = (RectTransform)chartItem.transform;
                itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemHeight);
            }

            // Content 高度（由于需要在整个区域接收指针输入，必须手动设置高度，不能依赖 Unity Content Size Fitter）：
            // - item <= BaseItemCount：Content 保持与 Viewport（lastRectSize）同高，item 从上往下排列，
            //   不足的部分表现为末尾空缺，前面 item 的位置与满 4 个时完全一致。
            // - item > BaseItemCount：Content 按实际 item 数量累加高度，此时 Content 高于 Viewport，可以上下滚动，
            //   末尾会多露出一个 item 的一部分。
            float contentHeight;
            if (itemCount <= BaseItemCount)
            {
                contentHeight = lastRectSize.y;
            }
            else
            {
                contentHeight = topMargin + bottomMargin + itemHeight * itemCount + itemGapHeight * (itemCount - 1);
            }

            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

            if (resetScrollPosition || itemCount <= BaseItemCount)
                contentRect.anchoredPosition = Vector2.zero;

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            UpdateChartItemHorizontalLayout();
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            UpdateChartItemHorizontalLayout();
        }

        /// <summary>
        /// 根据每个 item 在当前视口中的纵向位置，更新其 ChartItem 的横向归一化坐标。
        /// 纵向位置先按视口半高归一化，再由 CircularLayoutCalculator 反推椭圆角度并取右半椭圆横坐标，
        /// </summary>
        private void UpdateChartItemHorizontalLayout()
        {
            RectTransform selfRect = (RectTransform)transform;
            float halfViewportHeight = selfRect.rect.height * 0.5f;

            foreach (ChartItem chartItem in MetaDataToChartItemDict.Values)
            {
                RectTransform itemRect = (RectTransform)chartItem.transform;
                Vector3 worldCenter = itemRect.TransformPoint(itemRect.rect.center);
                Vector3 localCenter = selfRect.InverseTransformPoint(worldCenter);

                float normalizedY = halfViewportHeight > 0f ? localCenter.y / halfViewportHeight : 0f;
                chartItem.SetXPos(CircularLayoutCalculator.CalculateRightEllipseNormalizedX(
                    normalizedY,
                    circularLayout.Radius,
                    circularLayout.ScaleX
                ));
            }
        }


        /// <summary>
        /// 根据画布高度、边距、item 高度与间距比例以及基准 item 数量，
        /// 计算单个 item 的高度和 item 之间的间距高度。
        /// 计算规则：让 baseItemCount 个 item 恰好填满画布可用高度（画布高度减去上下边距）。
        /// </summary>
        /// <param name="itemHeight">计算得到的单个 item 高度（像素）。</param>
        /// <param name="itemGapHeight">计算得到的间距高度（像素）。</param>
        [Pure]
        private void CalculateHeight(out float itemHeight, out float itemGapHeight)
        {
            CalculateHeight(
                lastRectSize.y,
                topMargin,
                bottomMargin,
                itemHeightToSpacingHeightRatio,
                BaseItemCount,
                out itemHeight,
                out itemGapHeight
            );
        }

        /// <summary>
        /// 根据画布高度、边距、item 高度与间距比例以及基准 item 数量，
        /// 计算单个 item 的高度和 item 之间的间距高度。
        /// 计算规则：让 baseItemCount 个 item 恰好填满画布可用高度（画布高度减去上下边距）。
        /// </summary>
        /// <param name="canvasHeight">画布高度（像素）。</param>
        /// <param name="topMarginHeight">上边距高度（像素）。</param>
        /// <param name="bottomMarginHeight">下边距高度（像素）。</param>
        /// <param name="itemToGapRatio">item 高度与间距高度之比（大于 0）。</param>
        /// <param name="baseItemCount">作为基准的 item 数量（通常为 4）。</param>
        /// <param name="itemHeight">计算得到的单个 item 高度（像素）。</param>
        /// <param name="itemGapHeight">计算得到的间距高度（像素）。</param>
        [Pure]
        private static void CalculateHeight(
            float canvasHeight,
            float topMarginHeight,
            float bottomMarginHeight,
            float itemToGapRatio,
            int baseItemCount,
            out float itemHeight,
            out float itemGapHeight
        )
        {
            float availableHeight = Mathf.Max(0f, canvasHeight - topMarginHeight - bottomMarginHeight);
            float denominator = baseItemCount * itemToGapRatio + (baseItemCount - 1);
            itemGapHeight = availableHeight / denominator;
            itemHeight = itemToGapRatio * itemGapHeight;
        }
    }
}
