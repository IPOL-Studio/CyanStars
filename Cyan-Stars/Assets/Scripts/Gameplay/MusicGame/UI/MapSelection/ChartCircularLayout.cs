#nullable enable

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CyanStars.Chart;
using CyanStars.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面列表的纵向 ScrollView 布局。
    /// 以 4 个 item 恰好铺满玩家 Canvas 高度作为基准，结合 Canvas 高度与 item 高度/间距高度比反推 item 高度和间距；
    /// item 不足 4 个时向上对齐（缺失的 item 表现为末尾空缺），item 多于 4 个时继续向下追加并撑高 Content。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ChartCircularLayout : MonoBehaviour
    {
        [Header("依赖组件")]
        [SerializeField]
        private RectTransform scrollViewRect = null!;

        [SerializeField]
        private RectTransform viewportRect = null!;

        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private VerticalLayoutGroup contentVerticalLayoutGroup = null!;


        [Header("配置参数")]
        [Tooltip("上边距（px）")]
        [SerializeField]
        private float topMargin = 40f;

        [Tooltip("下边距（px）")]
        [SerializeField]
        private float bottomMargin = 40f;

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

        private ChartModule ChartModule =>
            chartModule ??= GameRoot.GetDataModule<ChartModule>();

        private RectTransform? selfRectTransform;

        private RectTransform SelfRectTransform =>
            selfRectTransform ??= (RectTransform)transform;

        private readonly Dictionary<ChartMetaData, RectTransform> MetaDataToTransformDict = new();

        // 缓存 RectTransform 大小状态
        private Vector2 lastRectSize;
        private bool isDirty = false;


        private void Start()
        {
            BuildLayout(ChartModule.SelectedRuntimeChartPack);
            ChartModule.OnSelectedChartPackChanged += RefreshChartLayout;
        }

        private void Update()
        {
            // 玩家屏幕高度变化后，在帧末统一重新计算布局，避免在尺寸变化回调中直接改布局
            if (isDirty)
                ApplyLayout(false);
        }

        private void OnDestroy()
        {
            ChartModule.OnSelectedChartPackChanged -= RefreshChartLayout;
            ReleaseLayout();
        }


        private void OnRectTransformDimensionsChange()
        {
            // 当 RectTransform 大小改变时，记录其大小，并设为脏数据
            Vector2 currentSize = SelfRectTransform.rect.size;
            if (!isDirty && currentSize != lastRectSize)
                isDirty = true;
            lastRectSize = currentSize;
        }

        private void RefreshChartLayout(RuntimeChartPack? runtimeChartPack)
        {
            ReleaseLayout();
            BuildLayout(runtimeChartPack);
        }

        private void ReleaseLayout()
        {
            foreach (RectTransform itemRect in MetaDataToTransformDict.Values)
            {
                if (itemRect == null)
                    continue;

                // 先禁用，确保本帧的自动布局不会再计算旧 item，随后延迟销毁
                itemRect.gameObject.SetActive(false);
                Destroy(itemRect.gameObject); // TODO: 使用对象池
            }

            MetaDataToTransformDict.Clear();
        }

        private void BuildLayout(RuntimeChartPack? runtimeChartPack)
        {
            if (runtimeChartPack == null)
                return;

            MetaDataToTransformDict.Clear();

            // 筛选出非空难度，实例化 go，填充到字典
            foreach (var chartMetaData in runtimeChartPack.ChartPackData.ChartMetaDatas)
            {
                if (chartMetaData.Difficulty != null)
                {
                    GameObject item = Instantiate(itemPrefab, contentRect); // TODO: 使用对象池
                    MetaDataToTransformDict[chartMetaData] = (RectTransform)item.transform;
                }
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
            lastRectSize = SelfRectTransform.rect.size;

            int itemCount = MetaDataToTransformDict.Count;

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
            foreach (RectTransform itemRect in MetaDataToTransformDict.Values)
            {
                if (itemRect == null)
                    continue;

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
