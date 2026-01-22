#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    public class ChartEditorModel
    {
        // == == 事件触发器 == ==
        /// <summary>
        /// BpmGroup 中既有元素的 StartBeat 或 Bpm 发生了变化时手动触发
        /// </summary>
        /// <remarks>
        ///     <para>参数为发生了变化的 bpmGroupItem 的 Index，代表需要刷新这个及后续 items</para>
        ///     <para>对列表元素的增删改不会触发此事件，而是由 ObservableList 处理</para>
        /// </remarks>
        public readonly Subject<int> BpmGroupDataChangedSubject = new();


        // == == 谱包和谱面数据 == ==

        // 元数据，在实例化 Model 时固定
        public readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        public readonly int ChartMetaDataIndex; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public readonly ReadOnlyReactiveProperty<ChartPackDataEditorModel> ChartPackData;
        public readonly ReadOnlyReactiveProperty<ChartDataEditorModel> ChartData;


        // == == 编辑器运行时数据 == ==

        // 编辑模式
        public readonly ReactiveProperty<bool> IsSimplificationMode = new ReactiveProperty<bool>(true);
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
        public int CurrentTimelineTime { get; set; } = 0; // 在音乐播放时由 ChartEditorMusicManager 负责每帧更新，暂停时由 EditAreaViewModel 负责在滚动 UI 时更新，考虑到性能问题由 view 在播放时轮询查询

        // 音符编辑
        // TODO: 后续用 list 拓展为选中多个音符一次编辑
        public readonly ReactiveProperty<BaseChartNoteData?> SelectedNoteData = new ReactiveProperty<BaseChartNoteData?>(null);

        // 弹窗可见性
        public readonly ReactiveProperty<bool> ChartPackDataCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> ChartDataCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> MusicVersionCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> BpmGroupCanvasVisibility = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> SpeedTemplateCanvasVisibility = new ReactiveProperty<bool>(false); // TODO

        // 编辑器属性
        public readonly ReactiveProperty<int> PosAccuracy = new ReactiveProperty<int>(4);
        public readonly ReactiveProperty<bool> PosMagnet = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<int> BeatAccuracy = new ReactiveProperty<int>(2);
        public readonly ReactiveProperty<float> BeatZoom = new ReactiveProperty<float>(1f);

        // 提示弹窗
        private readonly ReactiveProperty<bool> popupCanvasVisibility = new ReactiveProperty<bool>(false);
        public ReadOnlyReactiveProperty<bool> PopupCanvasVisibility => popupCanvasVisibility;
        public string PopupTitleString { get; private set; } = "";
        public string PopupDescribeString { get; private set; } = "";
        private readonly Dictionary<string, Action?> popupButtonCallBackMap = new Dictionary<string, Action?>();
        public IReadOnlyDictionary<string, Action?> PopupButtonCallBackMap => popupButtonCallBackMap;
        public bool PopupShowCloseButton { get; private set; } = true;


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


        public void ShowPopup(string title,
                              string describe,
                              Dictionary<string, Action?>? map = null,
                              bool showCloseButton = true)
        {
            if (popupCanvasVisibility.CurrentValue)
                throw new Exception("已经有一个弹窗打开了，不允许再开一个");

            PopupTitleString = title;
            PopupDescribeString = describe;
            popupButtonCallBackMap.Clear();
            if (map != null) // 如果 map 为 null，则等同空列表，不显示任何按钮
            {
                foreach (var kvp in map)
                {
                    popupButtonCallBackMap.Add(kvp.Key, kvp.Value);
                }
            }

            PopupShowCloseButton = showCloseButton;
            popupCanvasVisibility.Value = true; // 自动触发 UI 更新
        }

        public void ClosePopup()
        {
            if (!popupCanvasVisibility.CurrentValue)
                throw new Exception("弹窗已经关闭了，不能再次关闭");

            popupButtonCallBackMap.Clear(); // 准备立刻释放订阅
            popupCanvasVisibility.Value = false; // 自动触发 UI 更新
        }
    }
}
