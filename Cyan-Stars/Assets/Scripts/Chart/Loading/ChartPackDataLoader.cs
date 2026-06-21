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

namespace CyanStars.Chart.Loading
{
    /// <summary>
    /// 谱包加载器 — 负责谱包的批量/增量加载和重载
    /// </summary>
    public static class ChartPackDataLoader
    {
        /// <summary>
        /// 索引文件的文件名
        /// </summary>
        public const string ChartPackFileName = "ChartPack.json";

        /// <summary>
        /// 内置谱包 SO 文件路径
        /// </summary>
        private const string InternalChartPackListFilePath =
            "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

        /// <summary>
        /// 从磁盘重新加载全部谱包，包括内置和玩家谱包
        /// </summary>
        public static async Task<List<RuntimeChartPack>> ReloadAllAsync(string playerChartPacksFolderPath)
        {
            var paths = new List<string>();
            var levelsList = new List<ChartPackLevels>();

            using var internalListHandler = await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath);
            InternalChartPackListSO internalChartPackListSO = internalListHandler.Asset;
            foreach (var item in internalChartPackListSO.InternalChartPacks)
            {
                paths.Add(item.ChartPackFilePath);
                levelsList.Add(item.Levels);
            }

            int internalPacksCount = paths.Count;

            if (!Directory.Exists(playerChartPacksFolderPath))
            {
                Directory.CreateDirectory(playerChartPacksFolderPath);
            }

            var playerChartPaths =
                Directory.EnumerateFiles(playerChartPacksFolderPath, ChartPackFileName, SearchOption.AllDirectories);
            foreach (var path in playerChartPaths)
            {
                paths.Add(path.Replace('\\', '/'));
                levelsList.Add(new ChartPackLevels());
            }

            // 批量加载以提高效率，后续单个加载时能直接快速完成
            using var batchAssetHandler = await GameRoot.Asset.BatchLoadAssetAsync(paths);

            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < paths.Count; i++)
            {
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

                string? workspacePath = Path.GetDirectoryName(paths[i]);
                if (workspacePath == null)
                {
                    Debug.LogError($"谱包路径为空：{chartPackData.Title}，相关谱包无法加载！");
                    continue;
                }

                bool isInternal = i < internalPacksCount;
                if (isInternal && !VerifyInternalChartPackMetaData(chartPackData))
                {
                    Debug.LogWarning($"某个内置谱包元数据不正确：{chartPackData.Title}，当前已允许加载，正式发布时应当修复");
                }

                ChartPackLevels levels = levelsList[i];
                newPacks.Add(new RuntimeChartPack(chartPackData, isInternal, levels, workspacePath));
            }

            return newPacks;
        }

        /// <summary>
        /// 从磁盘加载一张谱包（视为玩家谱包）
        /// </summary>
        public static async Task<RuntimeChartPack> AddFromDiskAsync(string chartPackFilePath)
        {
            using var handler = await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath);
            ChartPackData? chartPackData = handler.Asset;

            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，加载失败！");

            Debug.Log($"已增量加载谱包 {chartPackFilePath}");
            return new RuntimeChartPack(
                chartPackData,
                false,
                new ChartPackLevels(),
                Path.GetDirectoryName(chartPackFilePath)
            );
        }

        /// <summary>
        /// 从磁盘重载某个玩家谱包的数据
        /// </summary>
        public static async Task<RuntimeChartPack> ReloadFromDiskAsync(string workspacePath)
        {
            string chartPackFilePath = PathUtil.Combine(workspacePath, ChartPackFileName);

            using var reloadHandler = await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath);
            ChartPackData? chartPackData = reloadHandler.Asset;
            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，增量更新失败！");

            Debug.Log($"已增量更新谱包 {chartPackFilePath}");
            return new RuntimeChartPack(
                chartPackData,
                false,
                new ChartPackLevels(),
                Path.GetDirectoryName(chartPackFilePath)
            );
        }

        /// <summary>
        /// 校验内置谱包下谱面是否合规
        /// </summary>
        /// <remarks>各个难度都有且仅有一张谱面，难度与下标一致，不存在 null 难度谱面</remarks>
        private static bool VerifyInternalChartPackMetaData(ChartPackData chartPackData)
        {
            return chartPackData.ChartMetaDatas.Count == 4 &&
                   chartPackData.ChartMetaDatas[0].Difficulty == ChartDifficulty.KuiXing &&
                   chartPackData.ChartMetaDatas[1].Difficulty == ChartDifficulty.QiMing &&
                   chartPackData.ChartMetaDatas[2].Difficulty == ChartDifficulty.TianShu &&
                   chartPackData.ChartMetaDatas[3].Difficulty == ChartDifficulty.WuYin;
        }
    }
}
