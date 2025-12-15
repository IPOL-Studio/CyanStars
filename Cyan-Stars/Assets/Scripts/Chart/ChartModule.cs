#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CatAsset.Runtime;
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
        private string PlayerChartPacksFolderPath { get; } = PathUtil.Combine(Application.persistentDataPath, "ChartPacks");


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
        /// 选中的谱面下标
        /// </summary>
        /// <remarks>为 null 时将在制谱器中创建新谱面</remarks>
        public int? SelectedChartIndex { get; private set; }

        /// <summary>
        /// 当前选中的谱包
        /// </summary>
        public RuntimeChartPack? SelectedRuntimeChartPack =>
            SelectedChartPackIndex != null ? runtimeChartPacks[(int)SelectedChartPackIndex] : null;

        /// <summary>
        /// 当前加载的谱面
        /// </summary>
        /// <remarks>外部调用时用 GetChartDataAsync()</remarks>
        private ChartData? chartData;

        private string? lastChartDataHash;
        private AssetHandler<ChartData>? lastChartDataHandler;


        public override async void OnInit()
        {
            SetTrackKeyToTypeMap();
        }


        /// <summary>
        /// 通过反射注册 特效轨道键-类 映射关系
        /// </summary>
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
        /// 清空已加载的谱包和谱面，并从磁盘重新加载全部谱包，包括内置和玩家谱包，不含谱面
        /// </summary>
        public async Task LoadRuntimeChartPacksFromDiskAsync()
        {
            runtimeChartPacks.Clear();

            var paths = new List<string>();
            var levelsList = new List<ChartPackLevels>();

            // 将内置谱包路径添加到列表
            InternalChartPackListSO internalChartPackListSO =
                (await GameRoot.Asset.LoadAssetAsync<InternalChartPackListSO>(InternalChartPackListFilePath)).Asset;
            foreach (var item in internalChartPackListSO.InternalChartPacks)
            {
                paths.Add(item.Path);
                levelsList.Add(item.Levels);
            }

            // 记录内置谱包的数量
            int internalPacksCount = paths.Count;

            // 将玩家谱包路径添加到列表
            if (!Directory.Exists(PlayerChartPacksFolderPath))
            {
                Directory.CreateDirectory(PlayerChartPacksFolderPath);
            }

            var playerChartPaths = Directory.EnumerateFiles(PlayerChartPacksFolderPath, ChartPackFileName,
                SearchOption.AllDirectories);
            foreach (var path in playerChartPaths)
            {
                paths.Add(path.Replace('\\', '/'));
                levelsList.Add(new ChartPackLevels());
            }

            // 批量加载谱包
            var batchAssetHandler = await GameRoot.Asset.BatchLoadAssetAsync(paths);

            // 校验加载的谱包数据有效性，并实例化 RuntimeChartPack
            var newPacks = new List<RuntimeChartPack>();
            for (int i = 0; i < paths.Count; i++)
            {
                ChartPackData chartPackData = batchAssetHandler.Handlers[i].AssetAs<ChartPackData>();
                if (batchAssetHandler.Handlers[i].AssetAs<ChartPackData>() == null)
                {
                    Debug.LogError($"无法将 {paths[i]} 转换为 {nameof(ChartPackData)}，相关谱包无法加载！");
                    continue;
                }

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

            runtimeChartPacks.AddRange(newPacks);
        }

        /// <summary>
        /// 选择一个谱包，并自动调整选定的谱面、音乐、难度
        /// </summary>
        /// <param name="index">新的谱包下标</param>
        public void SelectChartPackData(int index)
        {
            SelectedChartPackIndex = index;
            SelectedChartIndex = (RuntimeChartPacks[index].ChartPackData.ChartMetaDatas.Count >= 1) ? (int?)0 : null;
            SelectedMusicVersionIndex =
                (RuntimeChartPacks[index].ChartPackData.MusicVersionDatas.Count >= 1) ? (int?)0 : null;

            HashSet<ChartDifficulty> difficultiesAbleToPlay = RuntimeChartPacks[index].DifficultiesAbleToPlay;

            // 将难度设为上个谱包选中难度中最接近的那一个
            var currentDifficulty =
                RuntimeChartPacks[index].ChartPackData.ChartMetaDatas[(int)SelectedChartIndex].Difficulty;
            var difficulty = difficultiesAbleToPlay
                .OrderBy(e => Math.Abs(Convert.ToInt32(e) - (int)currentDifficulty))
                .ThenBy(e => Convert.ToInt32(e))
                .First();
        }

        /// <summary>
        /// 根据难度选中谱面
        /// </summary>
        /// <param name="difficulty">难度</param>
        public void SelectChartData(ChartDifficulty difficulty)
        {
            if (SelectedRuntimeChartPack is null)
            {
                Debug.LogError("尚未选择谱包，无法根据难度选择谱面。");
                return;
            }

            var difficultiesAbleToPlay = SelectedRuntimeChartPack.DifficultiesAbleToPlay;
            if (!difficultiesAbleToPlay.Contains(difficulty))
            {
                Debug.LogError($"此谱包的 {difficulty} 难度不可用！");
                return;
            }

            int index;
            for (index = 0; index < SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas.Count; index++)
            {
                if (SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[index].Difficulty == difficulty)
                {
                    break;
                }
            }

            SelectedChartIndex = index;
        }

        /// <summary>
        /// 根据下标选中谱面
        /// </summary>
        /// <param name="index">谱面下标</param>
        public void SelectChartData(int index)
        {
            if (SelectedRuntimeChartPack is null)
            {
                Debug.LogError("尚未选择谱包，无法根据难度选择谱面。");
                return;
            }

            if (index < 0 || index > SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            SelectedChartIndex = index;
        }

        /// <summary>
        /// 取消选中的谱包、音乐、谱面，并卸载已加载的谱面
        /// </summary>
        public void CancelSelectChartPackData()
        {
            SelectedChartPackIndex = null;
            SelectedMusicVersionIndex = null;
            SelectedChartIndex = null;
            CancelSelectChartData();
        }

        /// <summary>
        /// 取消选中的谱面，并卸载已加载的谱面
        /// </summary>
        public void CancelSelectChartData()
        {
            chartData = null;
            lastChartDataHash = null;
            lastChartDataHandler?.Unload();
            lastChartDataHandler = null;
        }

        /// <summary>
        /// 根据已选择的谱包和谱面下标加载谱面
        /// </summary>
        /// <returns>加载后的谱面数据</returns>
        public async Task<ChartData?> GetChartDataAsync()
        {
            if (SelectedRuntimeChartPack is null)
            {
                Debug.LogError("尚未选择谱包，无法加载谱面数据。");
                return null;
            }

            if (SelectedChartIndex is null)
            {
                Debug.LogError("尚未选择谱面，无法加载谱面数据。");
                return null;
            }

            // TODO：计算谱面哈希并校验/覆盖元数据内容，目前假定 metadata.ChartHash 提供了正确的 Hash

            ChartMetadata metadata = SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[(int)SelectedChartIndex];
            // if (lastChartDataHash != metadata.ChartHash)
            if (true) // TODO: 实现了 hash 计算后改用上面一行，暂时先每次都强制加载谱面
            {
                lastChartDataHandler?.Unload();
                string chartFilePath = PathUtil.Combine(SelectedRuntimeChartPack.WorkspacePath, metadata.FilePath);
                var handler = await GameRoot.Asset.LoadAssetAsync<ChartData>(chartFilePath);
                chartData = handler.Asset;

                if (chartData == null)
                {
                    Debug.LogError("获取谱面时异常，未加载");
                    return null;
                }

                Debug.Log("已加载了新的的谱面");
                lastChartDataHash = metadata.ChartHash;
                lastChartDataHandler = handler;
            }

            return chartData;
        }


        #region 谱包增量更新操作

        // 谱面无需增量更新，直接在更新谱包后 LoadChartDataFromDisk() 刷新即可

        /// <summary>
        /// 加载一个新的谱包到列表
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        /// <remarks>通过此方法增量加载的谱包视为玩家谱包</remarks>
        public async Task AddChartPackDataFromDisk(string chartPackFilePath)
        {
            var chartPackData = (await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath)).Asset;
            VerifyChartPacks(chartPackData, out bool canLoad, out HashSet<ChartDifficulty> difficultiesAbleToPlay);
            if (!canLoad)
            {
                Debug.LogError($"无法增量加载谱包 {chartPackFilePath}");
                return;
            }

            var runtimeChartPack =
                new RuntimeChartPack(chartPackData, false, new ChartPackLevels(), Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
            Debug.Log($"已增量加载谱包 {chartPackFilePath}");
            runtimeChartPacks.Add(runtimeChartPack);
        }

        /// <summary>
        /// 重载某个谱包的数据，见于制谱器更新时
        /// </summary>
        /// <param name="index">需要重载的谱包的下标</param>
        public async Task ReloadChartPackDataFromDisk(int index)
        {
            string chartPackFilePath = PathUtil.Combine(runtimeChartPacks[index].WorkspacePath, ChartPackFileName);

            var chartPackData = (await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath)).Asset;
            VerifyChartPacks(chartPackData, out bool canLoad, out HashSet<ChartDifficulty> difficultiesAbleToPlay);
            if (!canLoad)
            {
                Debug.LogError($"无法增量更新谱包 {chartPackFilePath}");
                return;
            }

            var runtimeChartPack =
                new RuntimeChartPack(chartPackData, false, new ChartPackLevels(), Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
            Debug.Log($"已增量更新谱包 {chartPackFilePath}");
            runtimeChartPacks[index] = runtimeChartPack;
        }

        #endregion

        // /// <summary>
        // /// 创建并选中新谱包，用于进入制谱器。
        // /// </summary>
        // /// <remarks>也会自动创建一张新的谱面</remarks>
        // /// <param name="folderName">文件夹名字和谱包标题，必须是一个合法的文件夹名字，且不存在重名</param>
        // /// <param name="errorMessage">失败信息</param>
        // /// <returns>是否成功创建并选中？</returns>
        // public bool CreateAndSelectNewChartPack(string folderName, out string errorMessage)
        // {
        //     errorMessage = "";
        //     try
        //     {
        //         if (!PathValidator.IsValidFolderName(folderName, out errorMessage))
        //         {
        //             Debug.LogWarning(errorMessage);
        //             return false;
        //         }
        //
        //         string workspacePath = PathUtil.Combine(PlayerChartPacksFolderPath, folderName);
        //         if (Directory.Exists(workspacePath))
        //         {
        //             errorMessage = "已经存同名文件夹，无法创建新的谱包";
        //             Debug.LogWarning(errorMessage);
        //             return false;
        //         }
        //
        //         // 实例化谱包数据和运行时谱包
        //         ChartPackData chartPackData = new ChartPackData(folderName);
        //         RuntimeChartPack runtimeChartPack = new RuntimeChartPack(chartPackData, false, new ChartPackLevels(),
        //             workspacePath, new HashSet<ChartDifficulty>());
        //
        //         // 选中新的谱包，并更新其他选中状态
        //         runtimeChartPacks.Add(runtimeChartPack);
        //         SelectedChartPackIndex = runtimeChartPacks.IndexOf(runtimeChartPack);
        //         SelectedChartDifficulty = null;
        //         SelectedChartIndex = null;
        //         SelectedMusicVersionIndex = null;
        //
        //         // 创建、保存并选中新的谱面
        //         if (!CreateAndSelectNewChart(runtimeChartPack, out errorMessage))
        //         {
        //             return false;
        //         }
        //
        //         // 序列化并保存索引文件
        //         Directory.CreateDirectory(workspacePath);
        //         GameRoot.File.SerializationToJson(runtimeChartPack.ChartPackData,
        //             PathUtil.Combine(workspacePath, ChartPackFileName));
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"无法创建并保存谱包：{e}");
        //         errorMessage = e.Message;
        //         return false;
        //     }
        // }
        //
        // /// <summary>
        // /// 创建并选中新谱面，用于进入制谱器。
        // /// </summary>
        // /// <remarks>也会自动在索引中注册文件位置。</remarks>
        // /// <param name="runtimeChartPack">运行时谱包实例</param>
        // /// <param name="errorMessage">失败信息</param>
        // /// <returns>是否成功创建并选中？</returns>
        // public bool CreateAndSelectNewChart(RuntimeChartPack runtimeChartPack, out string errorMessage)
        // {
        //     errorMessage = "";
        //     try
        //     {
        //         // 实例化谱面
        //         string workspacePath = runtimeChartPack.WorkspacePath;
        //         string chartsFolderAbsolutePath = PathUtil.Combine(workspacePath, "Charts");
        //         ChartData chartData = new ChartData();
        //
        //         // 向谱包索引文件中添加元数据，并选中此谱面
        //         int fileNameNumber = 0;
        //         while (File.Exists(PathUtil.Combine(chartsFolderAbsolutePath, $"Chart{fileNameNumber}.json")))
        //         {
        //             fileNameNumber++;
        //         }
        //
        //         string chartFileRelativePath = PathUtil.Combine("Charts", $"Chart{fileNameNumber}.json");
        //         string chartFileAbsolutePath = PathUtil.Combine(workspacePath, chartFileRelativePath);
        //
        //         ChartMetadata chartMetadata = new ChartMetadata(chartFileRelativePath);
        //         runtimeChartPack.ChartPackData.ChartMetaDatas.Add(chartMetadata);
        //         SelectedChartIndex = runtimeChartPack.ChartPackData.ChartMetaDatas.IndexOf(chartMetadata);
        //
        //         // 序列化并保存谱面文件
        //         Directory.CreateDirectory(chartsFolderAbsolutePath);
        //         GameRoot.File.SerializationToJson(chartData, chartFileAbsolutePath);
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"无法创建并保存谱面：{e}");
        //         errorMessage = e.Message;
        //         return false;
        //     }
        // }


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
