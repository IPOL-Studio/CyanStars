#nullable enable

using System;
using System.Diagnostics.Contracts;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework.File;
using CyanStars.Utils;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 制谱器主 Model 层
    /// </summary>
    public class ChartEditorModel : IDisposable
    {
        // == == 事件触发器 == ==
        /// <summary>
        /// BpmGroup 中既有元素的 StartBeat 或 Bpm 发生了变化时手动触发
        /// </summary>
        /// <remarks>
        ///     <para>参数为发生了变化的 bpmGroupItem 的 Index，代表需要刷新这个及后续 items</para>
        ///     <para>对列表元素的增删改不会触发此事件，而是由 ObservableList 处理</para>
        /// </remarks>
        public readonly Subject<int> BpmGroupDataChangedSubject = new Subject<int>();

        /// <summary>
        /// 选中的音符的 Pos/BreakPos/JudgeBeat/EndJudgeBeat 发生了变化时手动触发
        /// </summary>
        /// <remarks>用于刷新 EditArea 界面更新</remarks>
        public readonly Subject<BaseChartNoteData> SelectedNoteDataChangedSubject = new Subject<BaseChartNoteData>();

        // == == 谱包和谱面数据 == ==

        // 元数据，在实例化 Model 时固定
        public readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        public readonly int ChartMetaDataIndex; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public readonly ReadOnlyReactiveProperty<ChartPackDataEditorModel> ChartPackData;
        public readonly ReadOnlyReactiveProperty<ChartDataEditorModel> ChartData;

        /// <summary>
        /// 本次编辑会话的缓存区：导入的曲绘、音频先暂存在这里，保存时才写进工作区
        /// </summary>
        /// <remarks>归本 Model 独占，<see cref="Dispose"/> 时丢弃，不会影响下一次编辑</remarks>
        public readonly TempFileStore AssetStore;


        // == == 编辑器运行时数据 == ==

        // 编辑工具
        public readonly ReactiveProperty<EditToolType> SelectedEditTool = new ReactiveProperty<EditToolType>(EditToolType.Select);

        #region offset 说明

        // Offset 为正数代表在音乐前添加空白时间
        //                  |<-------------- 音乐时间 -------------->|
        // |<--- offset --->|
        // |<-------------------- timeline 时间 -------------------->|

        // Offset 为负数代表跳过一段音乐时间
        // |<-------------- 音乐时间 -------------->|
        // |<--- offset --->|
        //                  |<--- timeline 时间 --->|

        // 制谱器中始终以 timeline 时间为准

        #endregion

        // 音乐播放
        public readonly ReactiveProperty<bool> IsTimelinePlaying = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<AssetHandler<AudioClip?>?> AudioClipHandler = new ReactiveProperty<AssetHandler<AudioClip?>?>(null); // TODO: 在卸载 Model 时卸载 Handler
        public readonly ReactiveProperty<int> CurrentTimelineTimeMs = new ReactiveProperty<int>(0);
        public readonly ReactiveProperty<double> PlaybackSpeed = new ReactiveProperty<double>(1f);

        // 音符编辑
        // TODO: 后续用 list 拓展为选中多个音符一次编辑
        public readonly ReactiveProperty<BaseChartNoteData?> SelectedNoteData = new ReactiveProperty<BaseChartNoteData?>(null);

        // 当前打开的 Canvas 总数
        public int OpenCanvasCount = 0;

        // 制谱器属性
        public readonly ReactiveProperty<int> PosAccuracy = new ReactiveProperty<int>(4);
        public readonly ReactiveProperty<bool> PosMagnet = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<int> BeatAccuracy = new ReactiveProperty<int>(2);
        public readonly ReactiveProperty<double> BeatZoom = new ReactiveProperty<double>(1.0d);
        public readonly ReactiveProperty<float> ChartTracebackBeatOffset = new ReactiveProperty<float>(0f);

        // 制谱器设置
        public readonly ReactiveProperty<int> MusicVolume = new ReactiveProperty<int>(100);
        public readonly ReactiveProperty<int> NoteVolume = new ReactiveProperty<int>(100);
        public readonly ReactiveProperty<bool> IsChartTracebackEnabled = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> IsCompactNoteButtonArea = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> IsMultiBpmItemMode = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> IsMultiMusicItemMode = new ReactiveProperty<bool>(false);


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workspacePath">工作区绝对路径（谱包索引文件所在路径）</param>
        /// <param name="chartMetaDataIndex">谱面在谱包元数据中的索引</param>
        /// <param name="chartPackData">要修改的谱包数据，注意请先深拷贝一份</param>
        /// <param name="chartData">要修改的谱面数据，注意请先深拷贝一份</param>
        public ChartEditorModel(string workspacePath,
            int chartMetaDataIndex,
            ChartPackData chartPackData,
            ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartMetaDataIndex = chartMetaDataIndex;

            ChartPackData = new ReactiveProperty<ChartPackDataEditorModel>(new ChartPackDataEditorModel(chartPackData));
            ChartData = new ReactiveProperty<ChartDataEditorModel>(new ChartDataEditorModel(chartData));

            AssetStore = new TempFileStore("ChartEditor");
        }

        /// <summary>
        /// 工作区里的某个绝对路径是否仍被当前谱包数据引用（曲绘、任一音乐版本的音频）
        /// </summary>
        /// <param name="targetAbsolutePath">工作区里的资源绝对路径</param>
        [Pure]
        public bool IsAssetReferenced(string targetAbsolutePath)
        {
            if (string.IsNullOrEmpty(targetAbsolutePath))
                return false;

            foreach (string relativePath in ChartPackData.CurrentValue.GetAssetRelativePaths())
            {
                if (PathUtil.PathEquals(PathUtil.Combine(WorkspacePath, relativePath), targetAbsolutePath))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 结束本次编辑会话：卸载资源句柄、丢弃缓存区，未保存到工作区的导入文件一并删掉
        /// </summary>
        public void Dispose()
        {
            UnloadAssetHandlers();
            AssetStore.Discard();
        }

        /// <summary>
        /// 卸载本次会话加载的音乐资源
        /// </summary>
        /// <remarks>曲绘由 <c>ChartPackDataCoverViewModel</c> 自己卸载</remarks>
        private void UnloadAssetHandlers()
        {
            AudioClipHandler.CurrentValue?.Unload();
            AudioClipHandler.Value = null;
        }
    }
}
