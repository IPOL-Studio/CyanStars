#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Gameplay.MusicGame;
using CyanStars.Utils;
using UnityEngine;

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
        /// <remarks>内置谱包索引文件的位置（位于多媒体目录）和定数需要在这个文件中注册</remarks>
        private const string InternalChartPackListFilePath =
            "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

        /// <summary>
        /// 索引文件的文件名，将会在目录中查找此文件
        /// </summary>
        public const string ChartPackFileName = "ChartPack.json";

        /// <summary>
        /// 玩家谱包路径，位于用户数据
        /// </summary>
        private string PlayerChartPacksFolderPath { get; } = Path.Combine(Application.persistentDataPath, "ChartPacks");


        /// <summary>
        /// 谱面拓展轨道名称->轨道类型 映射表
        /// </summary>
        private Dictionary<string, Type> trackKeyToTypeMap;


        private List<RuntimeChartPack> runtimeChartPacks = new List<RuntimeChartPack>();

        /// <summary>
        /// 目前所有已加载的运行时谱包（只读）
        /// </summary>
        public IReadOnlyList<RuntimeChartPack> RuntimeChartPacks => runtimeChartPacks;

        /// <summary>
        /// 选中的谱包下标
        /// </summary>
        /// <remarks>为 null 时无法进入音游，但能在制谱器内创建新谱包</remarks>
        public int? SelectedChartPackIndex { get; private set; }

        /// <summary>
        /// 选中的音乐版本下标
        /// </summary>
        /// <remarks>为 null 时无法进入音游</remarks>
        public int? SelectedMusicVersionIndex { get; private set; }

        /// <summary>
        /// 选中的谱面难度（用于进入音游流程）
        /// </summary>
        /// <remarks>为 null 时无法进入音游</remarks>
        public ChartDifficulty? SelectedChartDifficulty { get; private set; }

        /// <summary>
        /// 选中的谱面下标（用于进入制谱器流程）
        /// </summary>
        /// <remarks>为 null 时将在制谱器中创建新谱面</remarks>
        public int? SelectedChartIndex { get; private set; }

        /// <summary>
        /// 当前选中的谱包
        /// </summary>
        public RuntimeChartPack? SelectedRuntimeChartPack =>
            SelectedChartPackIndex != null ? runtimeChartPacks[(int)SelectedChartPackIndex] : null;


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


        /// <summary>
        /// 从磁盘加载全部谱包清单，包括内置和玩家谱包，不含谱面
        /// </summary>
        public async Task LoadRuntimeChartPacksFromDisk()
        {
            var loadingTasks = new List<Task<ChartPackData>>();
            var paths = new List<string>();
            var levelsList = new List<ChartPackLevels>();

            // 将内置谱包添加到任务，并记录数量
            InternalChartPackListSO internalChartPackListSO =
                await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath);
            foreach (var item in internalChartPackListSO.InternalChartPacks)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(item.Path));
                paths.Add(item.Path);
                levelsList.Add(item.Levels);
            }

            int internalPacksCount = loadingTasks.Count;

            // 将玩家谱包添加到任务
            var playerChartPaths = Directory.EnumerateFiles(PlayerChartPacksFolderPath, ChartPackFileName,
                SearchOption.AllDirectories);
            foreach (var path in playerChartPaths)
            {
                loadingTasks.Add(GameRoot.Asset.LoadAssetAsync<ChartPackData>(path));
                paths.Add(path);
                levelsList.Add(new ChartPackLevels());
            }

            // 启动加载任务
            ChartPackData[] loadedChartPackData = await Task.WhenAll(loadingTasks);

            // 校验加载的谱包数据有效性，并实例化 RuntimeChartPack
            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < loadedChartPackData.Length; i++)
            {
                var chartPackData = loadedChartPackData[i];
                bool isInternal = i < internalPacksCount;

                string? workspacePath = Path.GetDirectoryName(paths[i]);
                if (workspacePath == null)
                {
                    Debug.LogError($"谱包路径为空：{chartPackData.Title}");
                    continue;
                }

                ChartPackLevels levels = levelsList[i];

                VerifyChartPacks(chartPackData, out bool canLoad, out HashSet<ChartDifficulty> difficultiesAbleToPlay);
                if (isInternal && (!canLoad || difficultiesAbleToPlay.Count != 4))
                {
                    Debug.LogError($"某个内置谱包加载失败或有无法游玩的难度：{chartPackData.Title}");
                }

                if (!canLoad)
                {
                    continue;
                }

                newPacks.Add(new RuntimeChartPack(chartPackData, isInternal, levels, workspacePath,
                    difficultiesAbleToPlay));
            }

            runtimeChartPacks.Clear();
            runtimeChartPacks.AddRange(newPacks);
        }

        /// <summary>
        /// 选择一个谱包，并自动调整选定的谱面、音乐、难度
        /// </summary>
        /// <param name="index">新的谱包下标</param>
        public void SelectChartPack(int index)
        {
            SelectedChartPackIndex = index;
            SelectedChartIndex = (RuntimeChartPacks[index].ChartPackData.ChartMetaDatas.Count >= 1) ? (int?)0 : null;
            SelectedMusicVersionIndex =
                (RuntimeChartPacks[index].ChartPackData.MusicVersionDatas.Count >= 1) ? (int?)0 : null;

            HashSet<ChartDifficulty> difficultiesAbleToPlay = RuntimeChartPacks[index].DifficultiesAbleToPlay;
            if (difficultiesAbleToPlay.Count == 0 || SelectedChartDifficulty == null)
            {
                SelectedChartDifficulty = null;
                return;
            }

            // 将难度设为上个谱包选中难度中最接近的那一个
            SelectedChartDifficulty = difficultiesAbleToPlay
                .OrderBy(e => Math.Abs(Convert.ToInt32(e) - (int)SelectedChartDifficulty))
                .ThenBy(e => Convert.ToInt32(e))
                .First();
        }

        /// <summary>
        /// 为当前选定的谱包设置难度
        /// </summary>
        /// <param name="difficulty">难度</param>
        public void SelectedDifficulty(ChartDifficulty difficulty)
        {
            if (!SelectedRuntimeChartPack.DifficultiesAbleToPlay.Contains(difficulty))
            {
                throw new Exception("无法选择难度。不存在对应谱面或存在多个谱面？");
            }

            SelectedChartDifficulty = difficulty;
        }

        /// <summary>
        /// 根据难度加载谱面
        /// </summary>
        /// <param name="runtimeChartPack">要加载的运行时谱包</param>
        /// <param name="difficulty">要加载的难度</param>
        /// <returns></returns>
        public async Task<ChartData> GetChartDataFromDisk(RuntimeChartPack runtimeChartPack, ChartDifficulty difficulty)
        {
            if (!runtimeChartPack.DifficultiesAbleToPlay.Contains(difficulty))
            {
                Debug.LogError("此难度不可游玩。缺少谱面或有多个谱面？");
                return null;
            }

            int index;
            for (index = 0; index < runtimeChartPack.ChartPackData.ChartMetaDatas.Count; index++)
            {
                if (runtimeChartPack.ChartPackData.ChartMetaDatas[index].Difficulty == difficulty)
                {
                    break;
                }
            }

            return await GetChartDataFromDisk(runtimeChartPack, index);
        }

        /// <summary>
        /// 根据下标加载谱面
        /// </summary>
        /// <param name="runtimeChartPack">要加载的运行时谱包</param>
        /// <param name="chartIndex">要加载的谱面下标</param>
        /// <returns>加载后的谱面数据</returns>
        public async Task<ChartData> GetChartDataFromDisk(RuntimeChartPack runtimeChartPack, int chartIndex)
        {
            // TODO：计算谱面哈希并校验/覆盖元数据内容
            ChartMetadata metadata = runtimeChartPack.ChartPackData.ChartMetaDatas[chartIndex];
            string chartFilePath = Path.Combine(runtimeChartPack.WorkspacePath, metadata.FilePath);
            ChartData chartData = await GameRoot.Asset.LoadAssetAsync<ChartData>(chartFilePath);
            if (chartData == null)
            {
                Debug.LogError("获取谱面时异常");
            }

            return chartData;
        }


        /// <summary>
        /// 创建并选中新谱包，用于进入制谱器。
        /// </summary>
        /// <remarks>也会自动创建一张新的谱面</remarks>
        /// <param name="folderName">文件夹名字和谱包标题，必须是一个合法的文件夹名字，且不存在重名</param>
        /// <param name="errorMessage">失败信息</param>
        /// <returns>是否成功创建并选中？</returns>
        public bool CreateAndSelectNewChartPack(string folderName, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                if (!PathValidator.IsValidFolderName(folderName, out errorMessage))
                {
                    Debug.LogWarning(errorMessage);
                    return false;
                }

                string workspacePath = Path.Combine(PlayerChartPacksFolderPath, folderName);
                if (Directory.Exists(workspacePath))
                {
                    errorMessage = "已经存同名文件夹，无法创建新的谱包";
                    Debug.LogWarning(errorMessage);
                    return false;
                }

                // 实例化谱包数据和运行时谱包
                ChartPackData chartPackData = new ChartPackData(folderName);
                RuntimeChartPack runtimeChartPack = new RuntimeChartPack(chartPackData, false, new ChartPackLevels(),
                    workspacePath, new HashSet<ChartDifficulty>());

                // 选中新的谱包，并更新其他选中状态
                runtimeChartPacks.Add(runtimeChartPack);
                SelectedChartPackIndex = runtimeChartPacks.IndexOf(runtimeChartPack);
                SelectedChartDifficulty = null;
                SelectedChartIndex = null;
                SelectedMusicVersionIndex = null;

                // 创建、保存并选中新的谱面
                if (!CreateAndSelectNewChart(runtimeChartPack, out errorMessage))
                {
                    return false;
                }

                // 序列化并保存索引文件
                Directory.CreateDirectory(workspacePath);
                GameRoot.File.SerializationToJson(runtimeChartPack.ChartPackData,
                    Path.Combine(workspacePath, ChartPackFileName));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"无法创建并保存谱包：{e}");
                errorMessage = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 创建并选中新谱面，用于进入制谱器。
        /// </summary>
        /// <remarks>也会自动在索引中注册文件位置。</remarks>
        /// <param name="runtimeChartPack">运行时谱包实例</param>
        /// <param name="errorMessage">失败信息</param>
        /// <returns>是否成功创建并选中？</returns>
        public bool CreateAndSelectNewChart(RuntimeChartPack runtimeChartPack, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                // 实例化谱面
                string workspacePath = runtimeChartPack.WorkspacePath;
                string chartsFolderAbsolutePath = Path.Combine(workspacePath, "Charts");
                ChartData chartData = new ChartData();

                // 向谱包索引文件中添加元数据，并选中此谱面
                int fileNameNumber = 0;
                while (File.Exists(Path.Combine(chartsFolderAbsolutePath, $"Chart{fileNameNumber}.json")))
                {
                    fileNameNumber++;
                }

                string chartFileRelativePath = Path.Combine("Charts", $"Chart{fileNameNumber}.json");
                string chartFileAbsolutePath = Path.Combine(workspacePath, chartFileRelativePath);

                ChartMetadata chartMetadata = new ChartMetadata(chartFileRelativePath);
                runtimeChartPack.ChartPackData.ChartMetaDatas.Add(chartMetadata);
                SelectedChartIndex = runtimeChartPack.ChartPackData.ChartMetaDatas.IndexOf(chartMetadata);

                // 序列化并保存谱面文件
                Directory.CreateDirectory(chartsFolderAbsolutePath);
                GameRoot.File.SerializationToJson(chartData, chartFileAbsolutePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"无法创建并保存谱面：{e}");
                errorMessage = e.Message;
                return false;
            }
        }


        /// <summary>
        /// 验证谱面文件
        /// </summary>
        /// <param name="chartPackData">谱包数据</param>
        /// <param name="canLoad">谱面能够加载</param>
        /// <param name="difficultiesAbleToPlay">可游玩的难度</param>
        private void VerifyChartPacks(ChartPackData chartPackData, out bool canLoad,
            out HashSet<ChartDifficulty> difficultiesAbleToPlay)
        {
            canLoad = false;
            difficultiesAbleToPlay = new HashSet<ChartDifficulty>();

            if (chartPackData == null)
            {
                Debug.LogError("ChartPackData 为空");
                return;
            }

            canLoad = true;
            CalculateDifficultiesCount(chartPackData, out _, out difficultiesAbleToPlay);
        }


        private readonly struct DifficultiesCount
        {
            public readonly int D0Count, D1Count, D2Count, D3Count, DNullCount;

            public DifficultiesCount(int d0Count, int d1Count, int d2Count, int d3Count, int dNullCount)
            {
                D0Count = d0Count;
                D1Count = d1Count;
                D2Count = d2Count;
                D3Count = d3Count;
                DNullCount = dNullCount;
            }
        }

        /// <summary>
        /// 统计谱包中各个难度的数量
        /// </summary>
        /// <param name="chartPackData">要检查的谱包</param>
        /// <param name="difficultiesCount">各难度的谱面计数</param>
        /// <param name="difficultiesAbleToPlay">可以游玩的难度</param>
        /// <returns>统计数据结构体</returns>
        private void CalculateDifficultiesCount(ChartPackData chartPackData,
            out DifficultiesCount difficultiesCount, out HashSet<ChartDifficulty> difficultiesAbleToPlay)
        {
            int c0 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.KuiXing);
            int c1 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.QiMing);
            int c2 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.TianShu);
            int c3 = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == ChartDifficulty.WuYin);
            int cN = chartPackData.ChartMetaDatas.Count(cmd => cmd.Difficulty == null);
            difficultiesCount = new DifficultiesCount(c0, c1, c2, c3, cN);

            difficultiesAbleToPlay = new HashSet<ChartDifficulty>();
            if (c0 == 1)
            {
                difficultiesAbleToPlay.Add(ChartDifficulty.KuiXing);
            }

            if (c1 == 1)
            {
                difficultiesAbleToPlay.Add(ChartDifficulty.QiMing);
            }

            if (c2 == 1)
            {
                difficultiesAbleToPlay.Add(ChartDifficulty.TianShu);
            }

            if (c3 == 1)
            {
                difficultiesAbleToPlay.Add(ChartDifficulty.WuYin);
            }
        }
    }
}
