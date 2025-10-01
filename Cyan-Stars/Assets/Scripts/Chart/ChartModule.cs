using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CyanStars.Framework;
using CyanStars.Gameplay.MusicGame;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包与谱面数据模块
    /// </summary>
    /// <remarks>提供给音游流程和制谱器流程使用。注意制谱器流程需要存一份深拷贝的谱包+谱面副本用于编辑。</remarks>
    public class ChartModule : BaseDataModule
    {
        /// <summary>
        /// 内置谱包 SO 文件路径
        /// </summary>
        /// <remarks>内置谱包索引文件的位置（位于）需要在这个文件中注册</remarks>
        private const string InternalChartPackListFilePath =
            "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

        private const string ChartPackFileName = "ChartPack.json";
        private string PlayerChartPacksFolderPath { get; } = Path.Combine(Application.persistentDataPath, "ChartPacks");


        private Dictionary<string, Type> trackKeyToTypeMap;
        private List<RuntimeChartPack> runtimeChartPacks = new List<RuntimeChartPack>();

        /// <summary>
        /// 所有已加载的谱包
        /// </summary>
        public IReadOnlyList<RuntimeChartPack> RuntimeChartPacks => runtimeChartPacks;

        /// <summary>
        /// 选中的谱包序号
        /// </summary>
        public int SelectedChartPackIndex { get; set; }

        /// <summary>
        /// 选中的谱面难度
        /// </summary>
        public ChartDifficulty? Difficulty { get; set; }

        /// <summary>
        /// 选中的音乐版本序号
        /// </summary>
        public int MusicVersionIndex { get; set; }


        public override async void OnInit()
        {
            SetTrackKeyToTypeMap();
            await LoadRuntimeChartPacksFromDisk();
        }

        private void SetTrackKeyToTypeMap()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            Dictionary<string, Type> tracks = new Dictionary<string, Type>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IChartTrackData).IsAssignableFrom(type))
                    {
                        var customAttributes = type.GetCustomAttributes<ChartTrackAttribute>(false);

                        foreach (var attribute in customAttributes)
                        {
                            tracks.Add(attribute.TrackKey, type);
                        }
                    }
                }
            }

            trackKeyToTypeMap = tracks;
        }

        public bool TryGetChartTrackType(string key, out Type type)
        {
            return trackKeyToTypeMap.TryGetValue(key, out type);
        }


        public async Task LoadRuntimeChartPacksFromDisk()
        {
            var loadingTasks = new List<Task<ChartPackData>>();
            var allPaths = new List<string>();

            // 将内置谱包添加到任务，并记录数量
            InternalChartPackListSO internalChartPackListSO =
                await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath);
            foreach (var path in internalChartPackListSO.InternalCharts)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(path));
                allPaths.Add(path);
            }

            int internalPacksCount = loadingTasks.Count;

            // 将玩家谱包添加到任务
            var playerChartPaths = Directory.EnumerateFiles(PlayerChartPacksFolderPath, ChartPackFileName,
                SearchOption.AllDirectories);
            foreach (var path in playerChartPaths)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(path));
                allPaths.Add(path);
            }

            // 启动加载任务
            ChartPackData[] loadedChartPackData = await Task.WhenAll(loadingTasks);

            // 校验加载的谱包数据有效性，并实例化 RuntimeChartPack
            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < loadedChartPackData.Length; i++)
            {
                var chartPackData = loadedChartPackData[i];
                bool isInternal = i < internalPacksCount;
                string workspacePath = Path.GetDirectoryName(allPaths[i]);

                if (VerifyChartPacks(chartPackData, isInternal) != VerifyState.Error)
                {
                    newPacks.Add(new RuntimeChartPack(chartPackData, isInternal, workspacePath));
                }
                else if (isInternal)
                {
                    throw new Exception($"某个内置谱包加载失败：{chartPackData.Title}");
                }
            }

            runtimeChartPacks.Clear();
            runtimeChartPacks.AddRange(newPacks);
        }

        /// <summary>
        /// 正式进入音游时加载谱面
        /// </summary>
        /// <remarks>重载方法</remarks>
        /// <param name="runtimeChartPack">运行时谱包</param>
        /// <param name="difficulty">要加载的谱面难度</param>
        /// <returns>加载后的谱面数据</returns>
        public async Task<ChartData> LoadChartDataFromDisk(RuntimeChartPack runtimeChartPack,
            ChartDifficulty difficulty)
        {
            int difficultyCount =
                runtimeChartPack.ChartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == difficulty);
            if (difficultyCount != 1)
            {
                Debug.LogError("此难度无对应谱面或存在多个谱面");
                return null;
            }

            for (int index = 0; index < runtimeChartPack.ChartPackData.ChartMetaDatas.Count; index++)
            {
                ChartMetadata cmd = runtimeChartPack.ChartPackData.ChartMetaDatas[index];
                if (cmd.Difficulty == difficulty)
                {
                    return await LoadChartDataFromDisk(runtimeChartPack, index);
                }
            }

            return null;
        }

        /// <summary>
        /// 正式进入音游时加载谱面
        /// </summary>
        /// <param name="runtimeChartPack">运行时谱包</param>
        /// <param name="chartMetaDataIndex">要加载的谱面元数据下标</param>
        /// <returns>加载后的谱面数据</returns>
        public async Task<ChartData> LoadChartDataFromDisk(RuntimeChartPack runtimeChartPack, int chartMetaDataIndex)
        {
            // TODO：计算谱面哈希并校验/覆盖元数据内容
            if (chartMetaDataIndex > runtimeChartPack.ChartPackData.ChartMetaDatas.Count - 1)
            {
                Debug.LogError("下标越界，无法加载谱面");
                return null;
            }

            ChartMetadata metadata = runtimeChartPack.ChartPackData.ChartMetaDatas[chartMetaDataIndex];
            string chartFilePath = Path.Combine(runtimeChartPack.WorkspacePath, metadata.FilePath);
            ChartData chartData = await GameRoot.Asset.LoadAssetAsync<ChartData>(chartFilePath);
            if (chartData == null)
            {
                Debug.LogError("获取谱面时异常");
            }

            return chartData;
        }

        /// <summary>
        /// 验证谱面文件
        /// </summary>
        /// <param name="chartPackData">谱包数据</param>
        /// <param name="isInternal">是否为内置谱包，内置谱包校验会更严格</param>
        /// <returns>校验结果</returns>
        private VerifyState VerifyChartPacks(ChartPackData chartPackData, bool isInternal)
        {
            if (chartPackData == null)
            {
                Debug.LogError("ChartPackData 为空");
                return VerifyState.Error;
            }

            int difficulty0Count = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.KuiXing);
            int difficulty1Count = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.QiMing);
            int difficulty2Count = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.TianShu);
            int difficulty3Count = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.WuYin);
            int difficultyNullCount = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == null);

            if (difficulty0Count > 1 || difficulty1Count > 1 || difficulty2Count > 1 || difficulty3Count > 1)
            {
                Debug.LogError("ChartPackData 某个难度有多个谱面");
                return VerifyState.Error;
            }

            int state = 0;
            if (difficulty0Count == 0 || difficulty1Count == 0 || difficulty2Count == 0 || difficulty3Count == 0)
            {
                if (isInternal)
                {
                    state = Math.Max(state, 2);
                    Debug.LogWarning("ChartPackData 某些难度谱面缺失");
                }
                else
                {
                    state = Math.Max(state, 1);
                    Debug.Log("ChartPackData 某些难度谱面缺失");
                }
            }

            if (difficultyNullCount > 0)
            {
                if (isInternal)
                {
                    state = Math.Max(state, 2);
                    Debug.LogWarning("ChartPackData 有未指定难度的冗余谱面");
                }
                else
                {
                    state = Math.Max(state, 1);
                    Debug.Log("ChartPackData 有未指定难度的冗余谱面");
                }
            }

            return (VerifyState)state;
        }

        private enum VerifyState
        {
            Success = 0, // 成功，没有任何问题
            Suggestion = 1, // 可以进一步修改或完善
            Warning = 2, // 有警告，但仍可加载
            Error = 3 // 不能加载的谱面
        }
    }
}
