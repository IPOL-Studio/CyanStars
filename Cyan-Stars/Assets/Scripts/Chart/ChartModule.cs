#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包与谱面数据模块
    /// </summary>
    /// <remarks>提供给音游流程和制谱器流程使用。注意制谱器流程需要存一份深拷贝的谱包+谱面副本用于编辑。</remarks>
    // ReSharper disable once ClassNeverInstantiated.Global // 由反射实例化
    public class ChartModule : BaseDataModule
    {
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
        /// 选择一个谱包，并自动调整选定的谱面、音乐、难度
        /// </summary>
        /// <param name="index">新的谱包下标</param>
        public void SelectChartPackData(int index)
        {
            SelectedChartPackIndex = index;
            SelectedChartMetadataIndex = (RuntimeChartPacks[index].ChartPackData.ChartMetaDatas.Count >= 1) ? 0 : null;
            SelectedMusicVersionIndex = (RuntimeChartPacks[index].ChartPackData.MusicVersionDatas.Count >= 1) ? 0 : null;
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
        }


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

            // TODO：计算谱面哈希并校验/覆盖元数据内容
            ChartData = await ChartLoadHelper.LoadChartDataAsync(SelectedRuntimeChartPack, (int)SelectedChartMetadataIndex);
        }


        /// <summary>
        /// 清空已加载的谱包和谱面，并从磁盘重新加载全部谱包，包括内置和玩家谱包，不含谱面
        /// </summary>
        public async Task ReloadAllChartPacksAsync()
        {
            runtimeChartPacks.Clear();
            runtimeChartPacks.AddRange(await ChartLoadHelper.ReloadAllChartPacksAsync(PlayerChartPacksFolderPath));
        }

        /// <summary>
        /// !!! 测试方法 !!! 卸载全部谱包并直接加载一张谱包
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        [Obsolete("仅用于 v0.2 制谱器测试，在搭建完选曲 UI 和加载逻辑后弃用此方法！")]
        public async Task SetChartPackDataFromDesk(string chartPackFilePath)
        {
            CancelSelectChartPackData();
            runtimeChartPacks.Clear();
            runtimeChartPacks.Add(await ChartLoadHelper.AddChartPackDataFromDisk(chartPackFilePath));
        }

        /// <summary>
        /// 加载一个新的谱包到列表
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        /// <remarks>通过此方法增量加载的谱包视为玩家谱包</remarks>
        public async Task AddChartPackDataFromDisk(string chartPackFilePath)
        {
            runtimeChartPacks.Add(await ChartLoadHelper.AddChartPackDataFromDisk(chartPackFilePath));
        }

        /// <summary>
        /// 重载某个谱包的数据，见于在制谱器更新了内容时
        /// </summary>
        /// <param name="index">需要重载的谱包的下标</param>
        public async Task ReloadChartPackDataFromDisk(int index)
        {
            runtimeChartPacks[index] =
                await ChartLoadHelper.ReloadChartPackDataFromDisk(runtimeChartPacks[index].WorkspacePath);
        }
    }
}
