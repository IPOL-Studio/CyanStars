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
    public class ChartModule : BaseDataModule
    {
        private const string InternalChartPackListFilePath =
            "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

        private string playerChartPacksFolderPath = Path.Combine(Application.persistentDataPath, "ChartPacks");

        private const string ChartPackFileName = "ChartPack.json";


        private Dictionary<string, Type> trackKeyToTypeMap;
        public List<RuntimeChartPack> RuntimeChartPacks = new List<RuntimeChartPack>();

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


        private async Task LoadRuntimeChartPacksFromDisk()
        {
            var loadingTasks = new List<Task<ChartPackData>>();
            InternalChartPackListSO internalChartPackListSO =
                await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath);

            foreach (var path in internalChartPackListSO.InternalCharts)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(path));
            }

            int internalPacksCount = loadingTasks.Count;

            var playerChartPaths = Directory.EnumerateFiles(playerChartPacksFolderPath, ChartPackFileName,
                SearchOption.AllDirectories);

            foreach (var path in playerChartPaths)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(path));
            }

            ChartPackData[] loadedChartPackData = await Task.WhenAll(loadingTasks);

            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < loadedChartPackData.Length; i++)
            {
                var chartPackData = loadedChartPackData[i];
                bool isInternal = i < internalPacksCount; // 判断是否是内置谱包

                if (VerifyChartPacks(chartPackData, isInternal) != VerifyState.Error)
                {
                    newPacks.Add(new RuntimeChartPack(chartPackData, isInternal));
                }
                else
                {
                    Debug.LogError($"谱包加载失败：{chartPackData.Title}");
                }
            }

            RuntimeChartPacks.Clear();
            RuntimeChartPacks.AddRange(newPacks);
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

            // TODO: 计算哈希
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
