#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Gameplay.MusicGame;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包与谱面加载静态工具类
    /// </summary>
    /// <remarks>
    /// 负责谱包的批量/增量加载、重载，以及单个谱面的加载。
    /// </remarks>
    public static class ChartLoadHelper
    {
        /// <summary>
        /// 内置谱包 SO 文件路径
        /// </summary>
        /// <remarks>内置谱包索引文件的位置（位于多媒体目录）和定数需要在这个文件中注册</remarks>
        private const string InternalChartPackListFilePath =
            "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

        /// <summary>
        /// 从磁盘重新加载全部谱包，包括内置和玩家谱包
        /// </summary>
        /// <param name="playerChartPacksFolderPath">玩家谱包文件夹路径</param>
        /// <returns>新加载的运行时谱包列表</returns>
        public static async Task<List<RuntimeChartPack>> ReloadAllChartPacksAsync(
            string playerChartPacksFolderPath)
        {
            var paths = new List<string>();
            var levelsList = new List<ChartPackLevels>();

            // 将内置谱包路径添加到列表
            using var internalListHandler = await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath);
            InternalChartPackListSO internalChartPackListSO = internalListHandler.Asset;
            foreach (var item in internalChartPackListSO.InternalChartPacks)
            {
                paths.Add(item.Path);
                levelsList.Add(item.Levels);
            }

            // 记录内置谱包的数量，后续可以通过 index 判断谱包是否为内置
            int internalPacksCount = paths.Count;

            // 将玩家谱包路径添加到列表
            if (!Directory.Exists(playerChartPacksFolderPath))
            {
                Directory.CreateDirectory(playerChartPacksFolderPath);
            }

            var playerChartPaths = Directory.EnumerateFiles(playerChartPacksFolderPath, ChartModule.ChartPackFileName,
                SearchOption.AllDirectories);
            foreach (var path in playerChartPaths)
            {
                paths.Add(path.Replace('\\', '/'));
                levelsList.Add(new ChartPackLevels());
            }

            // 批量预加载谱包
            using var batchAssetHandler = await GameRoot.Asset.BatchLoadAssetAsync(paths);

            // 加载并转换谱包
            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < paths.Count; i++)
            {
                // 采用 AssetHandler<TextAsset>，因为 BatchLoadAssetAsync 无法指定资源转换类型
                // 故批量加载后统一解析 json 以提高效率
                using AssetHandler<TextAsset> textHandler = GameRoot.Asset.LoadAssetAsync<TextAsset>(paths[i]);

                if (!textHandler.IsDone)
                    await textHandler;

                if (textHandler.Asset?.text == null)
                {
                    Debug.LogError($"无法将 {paths[i]} 转换为 {nameof(ChartPackData)}，相关谱包无法加载！");
                    continue;
                }

                ChartPackData? chartPackData = JsonLoadHelper.LoadData<ChartPackData>(textHandler.Asset.text);

                if (chartPackData == null)
                {
                    Debug.LogError($"无法将 {paths[i]} 转换为 {nameof(ChartPackData)}，相关谱包无法加载！");
                    continue;
                }

                // 工作区路径是谱包所在的绝对路径，后续资源、谱面等相对路径拼接在此路径之后
                // TODO: 安卓系统无法在读写区外直接拼接路径，需要将整包复制进读写区
                string? workspacePath = Path.GetDirectoryName(paths[i]);
                if (workspacePath == null)
                {
                    Debug.LogError($"谱包路径为空：{chartPackData.Title}，相关谱包无法加载！");
                    continue;
                }

                bool isInternal = i < internalPacksCount;
                HashSet<ChartDifficulty> difficultiesAbleToPlay = CalculateDifficultiesCount(chartPackData);
                if (isInternal && difficultiesAbleToPlay.Count != 4)
                {
                    // TODO: 正式发布时内置谱包难度数不等于 4 应抛异常
                    Debug.LogError($"某个内置谱包难度计数不等于 4：{chartPackData.Title}，当前已允许加载，正式发布时应当修复");
                }

                // levels 为各难度的定数，内置谱包由 Unity SO 配置，社区谱包固定为 0
                ChartPackLevels levels = levelsList[i];
                newPacks.Add(new RuntimeChartPack(chartPackData, isInternal, levels, workspacePath, difficultiesAbleToPlay));
            }

            return newPacks;
        }

        /// <summary>
        /// 从磁盘加载一张谱包
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        /// <returns>新加载的运行时谱包</returns>
        /// <remarks>通过此方法加载的谱包视为玩家谱包</remarks>
        public static async Task<RuntimeChartPack> AddChartPackDataFromDisk(string chartPackFilePath)
        {
            using var handler = await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath);
            ChartPackData? chartPackData = handler.Asset;

            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，加载失败！");

            HashSet<ChartDifficulty> difficultiesAbleToPlay = CalculateDifficultiesCount(chartPackData);

            Debug.Log($"已增量加载谱包 {chartPackFilePath}");
            return new RuntimeChartPack(chartPackData, false, new ChartPackLevels(),
                Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
        }

        /// <summary>
        /// 从磁盘重载某个谱包的数据
        /// </summary>
        /// <param name="workspacePath">谱包工作区的绝对路径</param>
        /// <returns>重载后的运行时谱包</returns>
        public static async Task<RuntimeChartPack> ReloadChartPackDataFromDisk(string workspacePath)
        {
            string chartPackFilePath = PathUtil.Combine(workspacePath, ChartModule.ChartPackFileName);

            using var reloadHandler = await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath);
            ChartPackData? chartPackData = reloadHandler.Asset;
            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，增量更新失败！");

            HashSet<ChartDifficulty> difficultiesAbleToPlay = CalculateDifficultiesCount(chartPackData);

            Debug.Log($"已增量更新谱包 {chartPackFilePath}");
            return new RuntimeChartPack(chartPackData, false, new ChartPackLevels(),
                Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
        }

        /// <summary>
        /// 加载谱面数据
        /// </summary>
        /// <param name="selectedPack">已选中的运行时谱包</param>
        /// <param name="chartMetadataIndex">谱面元数据下标</param>
        /// <returns>加载后的谱面数据，失败时返回 null</returns>
        public static async Task<ChartData?> LoadChartDataAsync(RuntimeChartPack selectedPack, int chartMetadataIndex)
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

        /// <summary>
        /// 统计谱包中各难度的唯一数量，返回可游玩的难度集合
        /// </summary>
        /// <param name="chartPackData">要检查的谱包</param>
        /// <returns>可游玩的难度集合（有且仅有一张对应谱面的难度）</returns>
        private static HashSet<ChartDifficulty> CalculateDifficultiesCount(ChartPackData chartPackData)
        {
            int c0 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.KuiXing);
            int c1 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.QiMing);
            int c2 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.TianShu);
            int c3 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.WuYin);

            var difficultiesAbleToPlay = new HashSet<ChartDifficulty>();
            if (c0 == 1) difficultiesAbleToPlay.Add(ChartDifficulty.KuiXing);
            if (c1 == 1) difficultiesAbleToPlay.Add(ChartDifficulty.QiMing);
            if (c2 == 1) difficultiesAbleToPlay.Add(ChartDifficulty.TianShu);
            if (c3 == 1) difficultiesAbleToPlay.Add(ChartDifficulty.WuYin);

            return difficultiesAbleToPlay;
        }
    }
}
