#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditAreaViewModel : BaseViewModel
    {
        private const float DefaultMajorBeatLineInterval = 250f;

        public readonly ReadOnlyReactiveProperty<int> BeatAccuracy;
        public readonly ReadOnlyReactiveProperty<float> BeatZoom;

        public readonly ReadOnlyReactiveProperty<float> ContentHeight;
        public readonly ReadOnlyReactiveProperty<float> TotalBeats;
        public readonly ReadOnlyReactiveProperty<int> PosLineCount;

        // 音符集合
        public readonly IObservableCollection<BaseChartNoteData> Notes;

        // 吸附设置
        public readonly ReadOnlyReactiveProperty<bool> PosMagnetState;
        public readonly ReadOnlyReactiveProperty<int> PosAccuracy;

        public EditAreaViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            BeatAccuracy = Model.BeatAccuracy.ToReadOnlyReactiveProperty();
            BeatZoom = Model.BeatZoom.ToReadOnlyReactiveProperty();
            Notes = Model.ChartData.CurrentValue.Notes;

            PosMagnetState = Model.PosMagnet.ToReadOnlyReactiveProperty().AddTo(base.Disposables);
            PosAccuracy = Model.PosAccuracy.ToReadOnlyReactiveProperty().AddTo(base.Disposables);

            // --- ContentHeight & TotalBeats 计算逻辑 (保持不变) ---
            ContentHeight = Observable
                .Merge(
                    Model.ChartPackData.CurrentValue.BpmGroup.ObserveChanged().Select(_ => Unit.Default),
                    Model.BpmGroupDataChangedSubject.Select(_ => Unit.Default)
                )
                .Prepend(Unit.Default)
                .CombineLatest(
                    Model.BeatZoom,
                    Model.AudioClipHandler,
                    (_, zoom, handler) =>
                    {
                        if (handler?.Asset is null) return 0f;
                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        if (BpmGroupHelper.Validate(bpmGroup) != BpmGroupHelper.BpmValidationStatus.Valid)
                            throw new Exception("Bpm 组数据异常");
                        float beatCount = BpmGroupHelper.CalculateBeat(bpmGroup, (int)(handler.Asset.length * 1000f));
                        return beatCount * DefaultMajorBeatLineInterval * zoom;
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            ReadOnlyReactiveProperty<int?> firstMusicVersionItemOffset =
                Model.ChartPackData.CurrentValue.MusicVersions
                    .ObserveChanged()
                    .Prepend((CollectionChangedEvent<MusicVersionDataEditorModel>)default)
                    .Select(_ => Model.ChartPackData.CurrentValue.MusicVersions.Count > 0
                        ? Model.ChartPackData.CurrentValue.MusicVersions[0]
                        : null)
                    .DistinctUntilChanged()
                    .Select(data => data != null
                        ? data.Offset.AsObservable().Select(x => (int?)x)
                        : Observable.Return((int?)null))
                    .Switch()
                    .ToReadOnlyReactiveProperty(null)
                    .AddTo(base.Disposables);

            TotalBeats = Observable
                .Merge(
                    Model.ChartPackData.CurrentValue.BpmGroup.ObserveChanged().Select(_ => Unit.Default),
                    Model.BpmGroupDataChangedSubject.Select(_ => Unit.Default)
                )
                .Prepend(Unit.Default)
                .CombineLatest(
                    firstMusicVersionItemOffset,
                    Model.AudioClipHandler,
                    (_, offset, handler) =>
                    {
                        if (offset == null || handler?.Asset is null) return 0;
                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        int totalTimelineTimeMs = (int)(handler.Asset.length * 1000) + (int)offset;
                        return BpmGroupHelper.CalculateBeat(bpmGroup, totalTimelineTimeMs);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            PosLineCount = Model.PosAccuracy
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<int>.Instance, 4)
                .AddTo(base.Disposables);
        }

        public float GetBeatLineDistance()
        {
            return DefaultMajorBeatLineInterval * BeatZoom.CurrentValue / BeatAccuracy.CurrentValue;
        }

        /// <summary>
        /// 计算点击位置对应的音符数据 (Pos, Beat)
        /// </summary>
        /// <param name="localPosition">相对于 Content 底部中心的坐标</param>
        /// <param name="judgeLineY">判定线在 Content 中的 Y 轴偏移</param>
        public (float pos, Beat beat)? CalculateNotePlacement(Vector2 localPosition, float judgeLineY)
        {
            // 1. 计算横坐标 (Pos) - 逻辑沿用旧代码
            float notePos;
            const float LeftBreakThreshold = -421f;
            const float RightBreakThreshold = 421f;
            const float CentralMin = -400f;
            const float CentralMax = 400f;

            if (localPosition.x <= LeftBreakThreshold) notePos = -1f; // Left Break
            else if (RightBreakThreshold <= localPosition.x) notePos = 2f; // Right Break
            else if (CentralMin <= localPosition.x && localPosition.x <= CentralMax)
            {
                // 中央轨道吸附逻辑
                if (PosMagnetState.CurrentValue)
                {
                    int posAcc = PosAccuracy.CurrentValue;
                    if (posAcc == 0)
                    {
                        notePos = 0.4f;
                    }
                    else
                    {
                        float subSectionWidth = (800f / (posAcc + 1)) / 2f;
                        float relativePosX = localPosition.x + 400f;
                        float snappingIndex = Mathf.Round(relativePosX / subSectionWidth);
                        float maxIndex = 2 * posAcc + 1;
                        snappingIndex = Mathf.Max(1f, Mathf.Min(maxIndex, snappingIndex));
                        float snappedRelativePos = snappingIndex * subSectionWidth;
                        float posX = snappedRelativePos - 400f;
                        notePos = (posX + 320f) / 800f;
                        notePos = Mathf.Min(notePos, 0.8f);
                        notePos = Mathf.Max(notePos, 0f);
                    }
                }
                else
                {
                    notePos = (localPosition.x + 320f) / 800f;
                    notePos = Mathf.Min(notePos, 0.8f);
                    notePos = Mathf.Max(notePos, 0f);
                }
            }
            else return null; // 点击了缝隙

            // 2. 计算纵坐标 (Beat)
            // localPosition.y 已经是相对于 Content 底部的距离
            float relativeY = localPosition.y - judgeLineY;
            float beatDistance = GetBeatLineDistance(); // 单个细分拍的距离

            // 转换为细分拍索引
            int subBeatIndex = Mathf.RoundToInt(relativeY / beatDistance);
            subBeatIndex = Mathf.Max(0, subBeatIndex);

            // 转换为 Beat 对象
            int acc = BeatAccuracy.CurrentValue;
            if (!Beat.TryCreateBeat(subBeatIndex / acc, subBeatIndex % acc, acc, out Beat noteBeat))
            {
                return null;
            }

            return (notePos, noteBeat);
        }

        public void CreateNote(float pos, Beat beat)
        {
            // 1. 获取当前选中的工具类型
            EditToolType currentTool = Model.SelectedEditTool.Value;

            // 2. 如果是“选择”或“橡皮擦”工具，则不创建音符
            if (currentTool == EditToolType.Select || currentTool == EditToolType.Eraser)
            {
                return;
            }

            // 3. 根据工具类型实例化对应的音符数据对象
            BaseChartNoteData noteData = null;

            switch (currentTool)
            {
                case EditToolType.TapPen:
                    noteData = new TapChartNoteData(pos, beat);
                    break;

                case EditToolType.DragPen:
                    noteData = new DragChartNoteData(pos, beat);
                    break;

                case EditToolType.ClickPen:
                    noteData = new ClickChartNoteData(pos, beat);
                    break;

                case EditToolType.HoldPen:
                    // Hold 音符需要结束时间。
                    // 默认创建时长度为 0 (Start == End)，后续由用户拖拽调整，或给予一个默认的短时长。
                    // 这里假设 Beat 是结构体(值类型)，直接赋值即可。
                    noteData = new HoldChartNoteData(pos, beat, beat);
                    break;

                case EditToolType.BreakPen:
                    // Break 音符通常使用枚举 (Left/Right) 而不是浮点 Pos。
                    // 根据之前的 View 逻辑：左侧 Break 对应 -1f，右侧 Break 对应 2f。
                    // 这里进行反向映射。
                    BreakNotePos breakPos = pos < 0.5f ? BreakNotePos.Left : BreakNotePos.Right;

                    noteData = new BreakChartNoteData(breakPos, beat);
                    break;
            }

            // 如果未能成功创建数据（例如未处理的枚举类型），直接返回
            if (noteData == null) return;

            // 4. 获取音符列表引用 (假设存储在 ChartPackData 的 Notes 集合中)
            var notesCollection = Model.ChartData.CurrentValue.Notes;

            // 5. 构建并执行命令 (支持撤销/重做)
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    execute: () =>
                    {
                        // 执行：将新音符添加到集合
                        notesCollection.Add(noteData);
                    },
                    undo: () =>
                    {
                        // 撤销：将新音符从集合移除
                        notesCollection.Remove(noteData);
                    }
                )
            );
        }
    }
}
