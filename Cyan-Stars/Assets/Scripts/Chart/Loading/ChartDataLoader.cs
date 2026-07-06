#nullable enable

using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Chart.Loading
{
    /// <summary>
    /// 谱面数据加载器 — 负责加载单个谱面数据
    /// </summary>
    public static class ChartDataLoader
    {
        /// <summary>
        /// 加载谱面数据
        /// </summary>
        /// <param name="selectedPack">已选中的运行时谱包</param>
        /// <param name="chartMetadataIndex">谱面元数据下标</param>
        /// <returns>加载后的谱面数据，失败时返回 null</returns>
        public static async Task<ChartData?> LoadAsync(RuntimeChartPack selectedPack, int chartMetadataIndex)
        {
            ChartMetaData metaData = selectedPack.ChartPackData.ChartMetaDatas[chartMetadataIndex];

            string chartFilePath = PathUtil.Combine(selectedPack.WorkspacePath, metaData.FilePath);
            using var chartHandler = await GameRoot.Asset.LoadAssetAsync<ChartData>(chartFilePath);

            if (chartHandler.Asset == null)
            {
                Debug.LogError($"无法将 {chartFilePath} 转换为 {nameof(ChartData)}，相关谱面无法加载！");
                return null;
            }

            Debug.Log("已加载了新的的谱面");
            return chartHandler.Asset;
        }
    }
}
