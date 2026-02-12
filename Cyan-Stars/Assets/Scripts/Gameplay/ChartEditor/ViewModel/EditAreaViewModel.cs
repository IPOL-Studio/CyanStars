// TODO: 待重构

#nullable enable

using System;
using System.Collections.Generic;
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
        public const float DefaultMajorBeatLineInterval = 250f;

        public Subject<BaseChartNoteData> SelectedNoteDataChangedSubject => Model.SelectedNoteDataChangedSubject;
        public ReadOnlyReactiveProperty<bool> IsTimelinePlaying => Model.IsTimelinePlaying;


        // 位置线
        public ReadOnlyReactiveProperty<int> BeatAccuracy => Model.BeatAccuracy;
        public ReadOnlyReactiveProperty<float> BeatZoom => Model.BeatZoom;

        // 节拍线和音符
        public readonly ReadOnlyReactiveProperty<float> ContentAddHeight; // 在原有的屏幕高度上再增加此高度
        public readonly ReadOnlyReactiveProperty<float> TotalBeats;
        public readonly ReadOnlyReactiveProperty<int> PosLineCount;
        public IReadOnlyObservableList<BaseChartNoteData> Notes => Model.ChartData.CurrentValue.Notes;
        private readonly List<HoldChartNoteData> holdNotes = new List<HoldChartNoteData>();
        public IReadOnlyList<HoldChartNoteData> HoldNotes => holdNotes; // 无序排列的 HoldNotes，用于全量遍历校验 holdNote 是否有位于可视范围内的部分
        private ReadOnlyReactiveProperty<bool> PosMagnetState => Model.PosMagnet;
        private ReadOnlyReactiveProperty<int> PosAccuracy => Model.PosAccuracy;


        public readonly ReadOnlyReactiveProperty<bool> CanPutNote;


        public EditAreaViewModel(ChartEditorModel model)
            : base(model)
        {
            Notes.ObserveAdd()
                .Subscribe(e =>
                    {
                        if (e.Value.Type == NoteType.Hold)
                        {
                            holdNotes.Add(e.Value as HoldChartNoteData);
                        }
                    }
                )
                .AddTo(base.Disposables);
            Notes.ObserveRemove()
                .Subscribe(e =>
                    {
                        if (e.Value.Type == NoteType.Hold)
                        {
                            holdNotes.Remove(e.Value as HoldChartNoteData);
                        }
                    }
                )
                .AddTo(base.Disposables);
            Notes.ObserveClear()
                .Subscribe(_ =>
                    {
                        holdNotes.Clear();
                    }
                )
                .AddTo(base.Disposables);
            Notes.ObserveReset()
                .Subscribe(_ =>
                    {
                        holdNotes.Clear();
                        foreach (var note in Notes)
                        {
                            if (note.Type == NoteType.Hold)
                            {
                                holdNotes.Add(note as HoldChartNoteData);
                            }
                        }
                    }
                )
                .AddTo(base.Disposables);

            // 更新 ContentAddHeight，当：
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - BeatZoom 更新
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
            ContentAddHeight = Observable
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
                        if (handler?.Asset == null)
                            return 0f;
                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        if (BpmGroupHelper.Validate(bpmGroup) != BpmGroupHelper.BpmValidationStatus.Valid)
                            throw new Exception("Bpm 组数据异常");
                        float beatCount = BpmGroupHelper.CalculateBeat(bpmGroup, (int)(handler.Asset.length * 1000f) + Model.ChartPackData.CurrentValue.MusicVersions[0].Offset.CurrentValue);
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

            // 更新 TotalBeats，当：
            // - firstMusicVersionItemOffset 发生了更新（MusicVersion 首个元素的索引发生更新，或首个元素内的 offset 属性更新）
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
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
                        if (offset == null || handler?.Asset == null) return 0;
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

            CanPutNote = Observable.Merge(
                    Model.ChartPackData.CurrentValue.MusicVersions.ObserveChanged().Select(_ => Unit.Default),
                    Model.ChartPackData.CurrentValue.BpmGroup.ObserveChanged().Select(_ => Unit.Default)
                )
                .Prepend(Unit.Default)
                .Select(_ => Model.ChartPackData.CurrentValue.MusicVersions.Count > 0 && Model.ChartPackData.CurrentValue.BpmGroup.Count > 0)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
        }

        /// <summary>
        /// 工厂方法：创建音符的子 ViewModel
        /// 将 protected 的 Model 和 CommandStack 传递给子 VM
        /// </summary>
        public EditAreaNoteViewModel CreateNoteViewModel(BaseChartNoteData noteData, float judgeLineYOffset)
        {
            return new EditAreaNoteViewModel(Model, noteData, this, judgeLineYOffset);
        }

        /// <summary>
        /// 获取两条主要（整拍）节拍线之间的像素距离
        /// </summary>
        public float GetMajorBeatLineDistance()
        {
            return DefaultMajorBeatLineInterval * BeatZoom.CurrentValue;
        }

        /// <summary>
        /// 获取两条细分节拍线之间的像素距离
        /// </summary>
        public float GetMinorBeatLineDistance()
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
            // 计算横坐标
            float notePos;
            const float leftBreakThreshold = -421f;
            const float rightBreakThreshold = 421f;
            const float centralMin = -400f;
            const float centralMax = 400f;

            if (localPosition.x <= leftBreakThreshold)
            {
                // Left Break
                notePos = -1f;
            }
            else if (rightBreakThreshold <= localPosition.x)
            {
                // Right Break
                notePos = 2f;
            }
            else if (centralMin <= localPosition.x && localPosition.x <= centralMax)
            {
                // 开启了横坐标吸附
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
                        snappingIndex = Mathf.Clamp(snappingIndex, 1f, maxIndex);
                        float snappedRelativePos = snappingIndex * subSectionWidth;
                        float posX = snappedRelativePos - 400f;
                        notePos = (posX + 320f) / 800f;
                        notePos = Mathf.Clamp(notePos, 0f, 0.8f);
                    }
                }
                else
                {
                    // 未开启位置吸附
                    notePos = (localPosition.x + 320f) / 800f;
                    notePos = Mathf.Clamp(notePos, 0f, 0.8f);
                }
            }
            else
            {
                // 点击了缝隙
                return null;
            }

            // 计算纵坐标
            float relativeY = localPosition.y - judgeLineY;
            float beatDistance = GetMinorBeatLineDistance(); // 单个细分拍的距离

            int subBeatIndex = Mathf.RoundToInt(relativeY / beatDistance);
            subBeatIndex = Mathf.Max(0, subBeatIndex);

            int acc = BeatAccuracy.CurrentValue;
            if (!Beat.TryCreateBeat(subBeatIndex / acc, subBeatIndex % acc, acc, out Beat noteBeat))
            {
                return null;
            }

            return (notePos, noteBeat);
        }

        /// <summary>
        /// 取消选中的音符（见于右键点击空白处）
        /// </summary>
        public void CancelSelectNote()
        {
            if (Model.SelectedNoteData.CurrentValue != null)
                Model.SelectedNoteData.Value = null;
        }

        /// <summary>
        /// 在指定位置创建音符
        /// </summary>
        public void CreateNote(float pos, Beat beat)
        {
            // 如果是“选择”或“橡皮擦”工具，则不创建音符，并取消选中音符
            EditToolType currentTool = Model.SelectedEditTool.Value;
            if (currentTool == EditToolType.Select || currentTool == EditToolType.Eraser)
            {
                if (Model.SelectedNoteData.CurrentValue != null)
                {
                    Model.SelectedNoteData.Value = null;
                }

                return;
            }

            // 根据工具类型实例化对应的音符数据对象
            BaseChartNoteData? noteData = null;

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
                    // 默认创建时长度为 0 (Start == End)，后续由用户调整。
                    noteData = new HoldChartNoteData(pos, beat, beat);
                    break;
                case EditToolType.BreakPen:
                    // Break 音符使用枚举而不是浮点 Pos。
                    // 左侧 Break 对应 -1f，右侧 Break 对应 2f。
                    BreakNotePos breakPos = pos < 0.5f ? BreakNotePos.Left : BreakNotePos.Right;
                    noteData = new BreakChartNoteData(breakPos, beat);
                    break;
            }

            if (noteData == null)
                throw new Exception("未能正确创建音符！");

            var notesCollection = Model.ChartData.CurrentValue.Notes;

            CommandStack.ExecuteCommand(
                () => NoteListHelper.TryInsertItem(notesCollection, noteData),
                () => notesCollection.Remove(noteData)
            );
        }

        public void TryUpdateTimelineTime(float contentY)
        {
            if (Model.AudioClipHandler.CurrentValue?.Asset == null)
                return;

            if (IsTimelinePlaying.CurrentValue)
            {
                // 正在播放时无需再次更新
                return;
            }

            // 判定线位置在计算中正好约掉，无需得知判定线位置
            float beatPrecent = -contentY / ContentAddHeight.CurrentValue;
            beatPrecent = Mathf.Clamp01(beatPrecent);
            int timelineTimeMs = BpmGroupHelper.CalculateTime(Model.ChartPackData.CurrentValue.BpmGroup, TotalBeats.CurrentValue * beatPrecent);
            Model.CurrentTimelineTimeMs = timelineTimeMs;
        }

        public float GetContentYByTimelineTime()
        {
            float currentFBeat = BpmGroupHelper.CalculateBeat(Model.ChartPackData.CurrentValue.BpmGroup, Model.CurrentTimelineTimeMs);
            float beatPrecent = currentFBeat / TotalBeats.CurrentValue;
            beatPrecent = Mathf.Clamp01(beatPrecent);
            return -beatPrecent * ContentAddHeight.CurrentValue;
        }


        public void OnSpaceDown()
        {
            if (Model.ChartPackData.CurrentValue.BpmGroup.Count == 0 || Model.AudioClipHandler.Value?.Asset == null)
            {
                if (Model.IsTimelinePlaying.CurrentValue)
                {
                    Model.IsTimelinePlaying.Value = false;
                }

                return;
            }

            Model.IsTimelinePlaying.Value = !Model.IsTimelinePlaying.Value;
        }
    }
}
