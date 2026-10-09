#nullable enable

using System;
using System.Collections.Generic;
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
        /// 本次编辑会话导入的资源句柄：目标绝对路径 → 句柄
        /// </summary>
        /// <remarks>句柄的生命周期与本次编辑会话相同：历史上引用过的句柄也会一直保留到会话结束，
        /// 这样撤销、重做时还能取回它们的缓存副本；<see cref="Dispose"/> 时统一释放</remarks>
        private readonly Dictionary<string, FileHandle> AssetHandles = new Dictionary<string, FileHandle>(PathUtil.PathComparer);

        /// <summary>
        /// 当前没有被谱包引用、但历史上引用过因而暂时保留的句柄
        /// </summary>
        /// <remarks>删除音乐版本等操作会解绑句柄，撤销时需要把它们取回来</remarks>
        private readonly List<FileHandle> DetachedAssetHandles = new List<FileHandle>();


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
        }

        /// <summary>
        /// 工作区里的某个绝对路径是否仍被当前谱包数据引用（曲绘、任一音乐版本的音频）
        /// </summary>
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
        /// 取工作区里的资源绝对路径
        /// </summary>
        /// <returns>绝对路径；相对路径为空时返回空字符串</returns>
        [Pure]
        public string GetAssetAbsolutePath(string relativePath)
        {
            return string.IsNullOrEmpty(relativePath)
                ? ""
                : PathUtil.Combine(WorkspacePath, relativePath);
        }

        /// <summary>
        /// 取某个资源当前可读取的路径：本会话导入过就用句柄里的缓存副本，否则用工作区里的文件
        /// </summary>
        /// <returns>可读路径；句柄失效时退化为工作区里的路径</returns>
        [Pure]
        public string ResolveAssetReadablePath(string relativePath)
        {
            string absolutePath = GetAssetAbsolutePath(relativePath);
            if (string.IsNullOrEmpty(absolutePath))
                return "";

            FileHandle? assetHandle = FindAssetHandle(absolutePath);

            return assetHandle != null && PlatformFilePortal.IsHandleReadable(assetHandle)
                ? assetHandle.ReadablePath
                : absolutePath;
        }

        /// <summary>
        /// 按目标绝对路径找本次会话导入的资源句柄
        /// </summary>
        /// <returns>句柄，本会话没有导入过时返回 null</returns>
        [Pure]
        public FileHandle? FindAssetHandle(string targetAbsolutePath)
        {
            return string.IsNullOrEmpty(targetAbsolutePath)
                ? null
                : AssetHandles.GetValueOrDefault(targetAbsolutePath);
        }

        /// <summary>
        /// 登记一个导入的资源句柄
        /// </summary>
        /// <remarks>
        /// <para>同一句柄至多登记在一个目标路径下：传入已在其它路径登记过的句柄时，先解除旧路径上的登记</para>
        /// <para>目标路径已被其它句柄占用时，被顶替的句柄转入解绑状态</para>
        /// </remarks>
        public void AddAssetHandle(string targetAbsolutePath, FileHandle assetHandle)
        {
            if (string.IsNullOrEmpty(targetAbsolutePath))
                throw new ArgumentException("目标路径为空", nameof(targetAbsolutePath));

            if (assetHandle == null)
                throw new ArgumentNullException(nameof(assetHandle));

            DetachedAssetHandles.Remove(assetHandle);
            RemovePreviousRegistration(assetHandle);

            if (AssetHandles.Remove(targetAbsolutePath, out FileHandle? replacedHandle) &&
                !ReferenceEquals(replacedHandle, assetHandle))
            {
                DetachedAssetHandles.Add(replacedHandle);
            }

            AssetHandles[targetAbsolutePath] = assetHandle;
        }

        /// <summary>
        /// 解除句柄在旧目标路径上的登记，保证一个句柄至多对应一个目标路径
        /// </summary>
        private void RemovePreviousRegistration(FileHandle assetHandle)
        {
            string? previousPath = null;

            foreach (var pair in AssetHandles)
            {
                if (!ReferenceEquals(pair.Value, assetHandle))
                    continue;

                previousPath = pair.Key;
                break;
            }

            if (previousPath != null)
                AssetHandles.Remove(previousPath);
        }

        /// <summary>
        /// 解绑某个资源句柄，句柄本身和它的缓存副本会保留到会话结束
        /// </summary>
        /// <returns>被解绑的句柄；本会话没有导入过时返回 null</returns>
        public FileHandle? DetachAssetHandle(string targetAbsolutePath)
        {
            if (string.IsNullOrEmpty(targetAbsolutePath))
                return null;

            if (!AssetHandles.Remove(targetAbsolutePath, out FileHandle? assetHandle))
                return null;

            DetachedAssetHandles.Add(assetHandle);
            return assetHandle;
        }

        /// <summary>
        /// 把之前解绑的句柄重新登记回目标路径
        /// </summary>
        /// <returns>是否登记成功</returns>
        public bool ReattachAssetHandle(string targetAbsolutePath, FileHandle assetHandle)
        {
            if (string.IsNullOrEmpty(targetAbsolutePath) || assetHandle == null)
                return false;

            DetachedAssetHandles.Remove(assetHandle);

            if (assetHandle.State == FileHandleState.Released)
                return false;

            AddAssetHandle(targetAbsolutePath, assetHandle);
            return true;
        }

        /// <summary>
        /// 取本次会话登记过的全部资源句柄
        /// </summary>
        [Pure]
        public List<FileHandle> GetAllAssetHandles()
        {
            return new List<FileHandle>(AssetHandles.Values);
        }

        /// <summary>
        /// 结束本次编辑会话：卸载资源句柄、释放全部缓存副本，未保存到工作区的导入文件一并删掉
        /// </summary>
        public void Dispose()
        {
            // 曲绘由 ChartPackDataCoverViewModel 自己卸载
            UnloadAudioAssetHandlers();

            PlatformFilePortal.TryReleaseAll(DetachedAssetHandles);
            PlatformFilePortal.TryReleaseAll(AssetHandles.Values);

            DetachedAssetHandles.Clear();
            AssetHandles.Clear();

            GameSessionTempFolder.DeleteCacheFolderIfEmpty(FileCacheKind.ChartEditor);
        }

        /// <summary>
        /// 卸载本次会话加载的音乐资源
        /// </summary>
        private void UnloadAudioAssetHandlers()
        {
            AudioClipHandler.CurrentValue?.Unload();
            AudioClipHandler.Value = null;
        }
    }
}
