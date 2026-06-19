#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Chart.Loading;
using CyanStars.Framework;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包与谱面数据模块
    /// </summary>
    /// <remarks>负责管理谱包列表、选中状态和谱面加载入口。注意制谱器流程需要存一份深拷贝的谱包+谱面副本用于编辑。</remarks>
    // ReSharper disable once ClassNeverInstantiated.Global // 由反射实例化
    public class ChartModule : BaseDataModule
    {
        /// <summary>
        /// 玩家谱包路径，位于用户数据
        /// </summary>
        public string PlayerChartPacksFolderPath { get; } =
            PathUtil.Combine(Application.persistentDataPath, "ChartPacks");

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

        /// <summary>
        /// 当选中的谱包或谱面发生了变化，T1=新选中的谱包，T2=新选中的谱面下标（谱包或谱面为空时记为-1）
        /// </summary>
        public Action<RuntimeChartPack?, int>? OnSelectedChartChanged;


        public override void OnInit()
        {
        }

        /// <summary>
        /// 选择一个谱包，并自动调整选定的音乐和谱面
        /// </summary>
        /// <param name="index">新的谱包下标</param>
        public void SelectChartPackData(int index)
        {
            CancelSelectChartData();
            SelectedChartPackIndex = index;
            SelectedChartMetadataIndex = RuntimeChartPacks[index].ChartPackData.ChartMetaDatas.Count >= 1 ? 0 : null;
            SelectedMusicVersionIndex = RuntimeChartPacks[index].ChartPackData.MusicVersionDatas.Count >= 1 ? 0 : null;
            _ = SelectChartDataAsync(0); // TODO: 记住玩家上次在此谱包中选择的谱面
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

            OnSelectedChartChanged?.Invoke(SelectedRuntimeChartPack, index);
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
            OnSelectedChartChanged?.Invoke(SelectedRuntimeChartPack, 0);
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

            CancelSelectChartData();

            ChartData = await ChartDataLoader.LoadAsync(SelectedRuntimeChartPack, (int)SelectedChartMetadataIndex);
        }


        /// <summary>
        /// 清空已加载的谱包和谱面，并从磁盘重新加载全部谱包，包括内置和玩家谱包，不含谱面
        /// </summary>
        public async Task ReloadAllChartPacksAsync()
        {
            runtimeChartPacks.Clear();
            runtimeChartPacks.AddRange(await ChartPackDataLoader.ReloadAllAsync(PlayerChartPacksFolderPath));
        }

        /// <summary>
        /// 加载一个新的谱包到列表
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        /// <remarks>通过此方法增量加载的谱包视为玩家谱包</remarks>
        public async Task AddChartPackDataFromDisk(string chartPackFilePath)
        {
            runtimeChartPacks.Add(await ChartPackDataLoader.AddFromDiskAsync(chartPackFilePath));
        }

        /// <summary>
        /// 重载某个谱包的数据，见于在制谱器更新了内容时
        /// </summary>
        /// <param name="index">需要重载的谱包的下标</param>
        public async Task ReloadChartPackDataFromDisk(int index)
        {
            runtimeChartPacks[index] =
                await ChartPackDataLoader.ReloadFromDiskAsync(runtimeChartPacks[index].WorkspacePath);
        }

        /// <summary>
        /// 清空所有谱包并加载一张谱包（仅用于测试）
        /// </summary>
        /// <param name="chartPackFilePath">谱包索引文件的绝对路径</param>
        [Obsolete("仅用于测试，待选曲 UI 完善后删除")]
        public async Task SetSingleChartPackFromDisk(string chartPackFilePath)
        {
            runtimeChartPacks.Clear();
            runtimeChartPacks.Add(await ChartPackDataLoader.AddFromDiskAsync(chartPackFilePath));
        }
    }
}
