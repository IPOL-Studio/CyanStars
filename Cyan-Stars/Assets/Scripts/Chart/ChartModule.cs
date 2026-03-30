#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public string PlayerChartPacksFolderPath { get; } = PathUtil.Combine(Application.persistentDataPath, "ChartPacks");


        /// <summary>
        /// 谱面拓展轨道名称->轨道类型 映射表
        /// </summary>
        private readonly Dictionary<string, Type> TrackKeyToTypeMap = new();


        private readonly List<RuntimeChartPack> runtimeChartPacks = new();

        /// <summary>
        /// 目前所有已加载的运行时谱包（只读列表）
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
        /// 选中的谱面元数据下标
        /// </summary>
        /// <remarks>为 null 时将在制谱器中创建新谱面</remarks>
        public int? SelectedChartMetadataIndex { get; private set; }

        /// <summary>
        /// 当前选中的谱包
        /// </summary>
        public RuntimeChartPack? SelectedRuntimeChartPack =>
            SelectedChartPackIndex != null ? runtimeChartPacks[(int)SelectedChartPackIndex] : null;

        /// <summary>
        /// 当前加载的谱面
        /// </summary>
        public ChartData? ChartData { get; private set; }

        private string? lastChartDataHash;


        public override void OnInit()
        {
            SetTrackKeyToTypeMap();
        }


        /// <summary>
        /// 通过反射注册 特效轨道键-类 映射关系
        /// </summary>
        private void SetTrackKeyToTypeMap()
        {
            TrackKeyToTypeMap.Clear();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract && typeof(IChartTrackData).IsAssignableFrom(type))
                    {
                        var customAttributes = type.GetCustomAttributes<ChartTrackAttribute>(false);

                        foreach (var attribute in customAttributes)
                        {
                            TrackKeyToTypeMap.Add(attribute.TrackKey, type);
                        }
                    }
                }
            }
        }

        public bool TryGetChartTrackType(string key, out Type type)
        {
            return TrackKeyToTypeMap.TryGetValue(key, out type);
        }


        /// <summary>
        /// 清空已加载的谱包和谱面，并从磁盘重新加载全部谱包，包括内置和玩家谱包，不含谱面
        /// </summary>
        public async Task ReloadAllChartPacksAsync()
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

            // 记录内置谱包的数量，这样子后续就可以通过 index 判断谱包是不是内置的了
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

            // 批量预加载谱包
            using var batchAssetHandler = await GameRoot.Asset.BatchLoadAssetAsync(paths);

            // 加载并转换谱包
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

                // 工作区路径是谱包所在的绝对路径，后续相关的资源、谱面等相对路径直接拼接在工作区路径之后即可
                string? workspacePath = Path.GetDirectoryName(paths[i]);
                if (workspacePath == null)
                {
                    Debug.LogError($"谱包路径为空：{chartPackData.Title}，相关谱包无法加载！");
                    continue;
                }

                bool isInternal = i < internalPacksCount;
                CalculateDifficultiesCount(chartPackData, out _, out HashSet<ChartDifficulty> difficultiesAbleToPlay);
                if (isInternal && difficultiesAbleToPlay.Count != 4)
                {
                    // TODO: 正式发布时在内置谱包谱面数不等于 4 时抛异常
                    Debug.LogError($"某个内置谱包难度计数不等于 4：{chartPackData.Title}，当前已允许加载，正式发布时应当修复");
                }

                // levels 为各个难度的定数，内置谱包此值由 Unity SO 配置，社区谱包此值固定为 0
                ChartPackLevels levels = levelsList[i];
                newPacks.Add(new RuntimeChartPack(chartPackData, isInternal, levels, workspacePath, difficultiesAbleToPlay));
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
            SelectedChartMetadataIndex = (RuntimeChartPacks[index].ChartPackData.ChartMetaDatas.Count >= 1) ? 0 : null;
            SelectedMusicVersionIndex = (RuntimeChartPacks[index].ChartPackData.MusicVersionDatas.Count >= 1) ? 0 : null;

            HashSet<ChartDifficulty> difficultiesAbleToPlay = RuntimeChartPacks[index].DifficultiesAbleToPlay;

            // // TODO:将难度设为上个谱包选中难度中最接近的那一个
            // var currentDifficulty =
            //     RuntimeChartPacks[index].ChartPackData.ChartMetaDatas[(int)SelectedChartMetadataIndex].Difficulty;
            // var difficulty = difficultiesAbleToPlay
            //     .OrderBy(e => Math.Abs(Convert.ToInt32(e) - (int)currentDifficulty))
            //     .ThenBy(e => Convert.ToInt32(e))
            //     .First();
        }

        /// <summary>
        /// 根据难度选中谱面
        /// </summary>
        /// <param name="difficulty">难度</param>
        public async Task SelectChartDataAsync(ChartDifficulty difficulty)
        {
            if (SelectedRuntimeChartPack == null)
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

            SelectedChartMetadataIndex = index;
            await LoadChartDataAsync();
        }

        /// <summary>
        /// 根据下标选中谱面
        /// </summary>
        /// <param name="index">谱面下标</param>
        public async Task SelectChartDataAsync(int index)
        {
            if (SelectedRuntimeChartPack == null)
            {
                Debug.LogError("尚未选择谱包，无法根据难度选择谱面。");
                return;
            }

            if (index < 0 || index > SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas.Count - 1)
            {
                throw new IndexOutOfRangeException();
            }

            SelectedChartMetadataIndex = index;
            await LoadChartDataAsync();
        }

        /// <summary>
        /// 取消选中的谱包、音乐、谱面，并卸载已加载的谱面
        /// </summary>
        public void CancelSelectChartPackData()
        {
            SelectedChartPackIndex = null;
            SelectedMusicVersionIndex = null;
            SelectedChartMetadataIndex = null;
            CancelSelectChartData();
        }

        /// <summary>
        /// 取消选中的谱面，并卸载已加载的谱面
        /// </summary>
        public void CancelSelectChartData()
        {
            ChartData = null;
            lastChartDataHash = null;
        }

        /// <summary>
        /// 根据已选择的谱包和谱面下标加载谱面
        /// </summary>
        /// <returns>加载后的谱面数据</returns>
        private async Task LoadChartDataAsync()
        {
            if (SelectedRuntimeChartPack == null)
            {
                Debug.LogError("尚未选择谱包，无法加载谱面数据。");
                return;
            }

            if (SelectedChartMetadataIndex == null)
            {
                Debug.LogError("尚未选择谱面，无法加载谱面数据。");
                return;
            }

            // TODO：计算谱面哈希并校验/覆盖元数据内容，目前假定 metaData.ChartHash 提供了正确的 Hash

            ChartMetaData metaData = SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[(int)SelectedChartMetadataIndex];
            // if (lastChartDataHash != metaData.ChartHash)
            if (true) // TODO: 实现了 hash 计算后改用上面一行，暂时先每次都强制加载谱面
            {
                string chartFilePath = PathUtil.Combine(SelectedRuntimeChartPack.WorkspacePath, metaData.FilePath);
                using var textHandler = await GameRoot.Asset.LoadAssetAsync<TextAsset>(chartFilePath);

                if (textHandler.Asset?.text == null)
                {
                    Debug.LogError($"无法将 {chartFilePath} 转换为 {nameof(ChartData)}，相关谱面无法加载！");
                    return;
                }

                ChartData? chartData = await Task.Run(() => JsonLoadHelper.LoadData<ChartData>(textHandler.Asset.text));
                if (chartData == null)
                {
                    Debug.LogError($"无法将 {chartFilePath} 转换为 {nameof(ChartData)}，相关谱面无法加载！");
                    return;
                }

                Debug.Log("已加载了新的的谱面");
                ChartData = chartData;
                lastChartDataHash = metaData.ChartHash;
            }
        }


        #region 谱包增量更新操作

        // 谱面无需增量更新，直接在更新谱包后 LoadChartDataFromDisk() 刷新即可

        /// <summary>
        /// !!! 测试方法 !!! 卸载全部谱包并直接加载一张谱包
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        [Obsolete("仅用于 Beta2 制谱器测试，在搭建完选曲 UI 和加载逻辑后弃用此方法！")]
        public async Task SetChartPackDataFromDesk(string chartPackFilePath)
        {
            CancelSelectChartPackData();
            runtimeChartPacks.Clear();

            await AddChartPackDataFromDisk(chartPackFilePath);
        }

        /// <summary>
        /// 加载一个新的谱包到列表
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        /// <remarks>通过此方法增量加载的谱包视为玩家谱包</remarks>
        public async Task AddChartPackDataFromDisk(string chartPackFilePath)
        {
            ChartPackData? chartPackData = (await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath)).Asset;

            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，加载失败！");

            CalculateDifficultiesCount(chartPackData, out _, out HashSet<ChartDifficulty> difficultiesAbleToPlay);

            // 社区谱包不参与玩家潜力值计算，定数为 0
            var runtimeChartPack =
                new RuntimeChartPack(chartPackData, false, new ChartPackLevels(), Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
            Debug.Log($"已增量加载谱包 {chartPackFilePath}");
            runtimeChartPacks.Add(runtimeChartPack);
        }

        /// <summary>
        /// 重载某个谱包的数据，见于在制谱器更新了内容时
        /// </summary>
        /// <param name="index">需要重载的谱包的下标</param>
        public async Task ReloadChartPackDataFromDisk(int index)
        {
            string chartPackFilePath = PathUtil.Combine(runtimeChartPacks[index].WorkspacePath, ChartPackFileName);

            ChartPackData? chartPackData = (await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath)).Asset;
            if (chartPackData == null)
                throw new NullReferenceException("尝试加载的谱包为 null，增量更新失败！");

            CalculateDifficultiesCount(chartPackData, out _, out HashSet<ChartDifficulty> difficultiesAbleToPlay);

            // 社区谱包不参与玩家潜力值计算，定数为 0
            var runtimeChartPack =
                new RuntimeChartPack(chartPackData, false, new ChartPackLevels(), Path.GetDirectoryName(chartPackFilePath), difficultiesAbleToPlay);
            Debug.Log($"已增量更新谱包 {chartPackFilePath}");
            runtimeChartPacks[index] = runtimeChartPack;
        }

        #endregion

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
                                                out DifficultiesCount difficultiesCount,
                                                out HashSet<ChartDifficulty> difficultiesAbleToPlay)
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
