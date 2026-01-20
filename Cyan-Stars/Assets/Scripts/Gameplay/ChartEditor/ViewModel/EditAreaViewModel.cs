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

        public ReadOnlyReactiveProperty<bool> PosMagnetState => Model.PosMagnet;
        public ReadOnlyReactiveProperty<int> PosAccuracy => Model.PosAccuracy;

        // 位置线
        public readonly ReadOnlyReactiveProperty<int> BeatAccuracy;
        public readonly ReadOnlyReactiveProperty<float> BeatZoom;

        // 节拍线和音符
        public readonly ReadOnlyReactiveProperty<float> ContentHeight;
        public readonly ReadOnlyReactiveProperty<float> TotalBeats;
        public readonly ReadOnlyReactiveProperty<int> PosLineCount;
        public IReadOnlyObservableList<BaseChartNoteData> Notes => Model.ChartData.CurrentValue.Notes;


        public EditAreaViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            BeatAccuracy = Model.BeatAccuracy.ToReadOnlyReactiveProperty();
            BeatZoom = Model.BeatZoom.ToReadOnlyReactiveProperty();

            // 更新 ContentHeight，当：
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - BeatZoom 更新
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
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
                        if (handler?.Asset is null)
                            return 0f;

                        var bpmGroup = Model.ChartPackData.CurrentValue.BpmGroup;
                        if (BpmGroupHelper.Validate(bpmGroup) != BpmGroupHelper.BpmValidationStatus.Valid)
                            throw new Exception("Bpm 组数据异常");

                        float beatCount =
                            BpmGroupHelper.CalculateBeat(bpmGroup, (int)(handler.Asset.length * 1000f));
                        return beatCount * DefaultMajorBeatLineInterval * zoom;
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            // 更新 TotalBeats，当：
            // - firstMusicVersionItemOffset 发生了更新（MusicVersion 首个元素的索引发生更新，或首个元素内的 offset 属性更新）
            // - BpmGroup 列表更新（元素增加、删除、移动）
            // - 手动触发了 BpmGroupDataChangedSubject（BpmGroup 中某一元素的 bpm 或 startBeat 更新）
            // - AudioClipHandler 更新（音乐变化后音频时长可能改变）
            // （bro，这下面的代码可太炸裂了...）
            ReadOnlyReactiveProperty<int?> firstMusicVersionItemOffset = // 始终监听到第一个 MusicVersionItem 的 offset 变化
                Model.ChartPackData.CurrentValue.MusicVersions
                    .ObserveChanged()
                    .Prepend((CollectionChangedEvent<MusicVersionDataEditorModel>)default)
                    .Select(_ =>
                        Model.ChartPackData.CurrentValue.MusicVersions.Count > 0
                            ? Model.ChartPackData.CurrentValue.MusicVersions[0]
                            : null
                    )
                    .DistinctUntilChanged()
                    .Select(data => data != null
                        ? data.Offset.AsObservable().Select(x => (int?)x)
                        : Observable.Return((int?)null)
                    )
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
                        if (offset == null || handler?.Asset is null)
                            return 0;

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
        /// 计算点击位置对应的音符 Beat 和 Pos
        /// </summary>
        public (float pos, Beat beat)? CalculateNotePlacement(Vector2 localPosition, float judgeLineY)
        {
            // 计算横坐标
            float notePos;

            if (localPosition.x <= -421f)
                notePos = -1f; // Left Break
            else if (421f <= localPosition.x)
                notePos = 2f; // Right Break
            else if (-400f <= localPosition.x && localPosition.x <= 400f)
            {
                // 中央轨道
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
                    notePos = (localPosition.x + 320f) / 800f;
                    notePos = Mathf.Clamp(notePos, 0f, 0.8f);
                }
            }
            else
                return null; // 点击了缝隙

            // 计算纵坐标
            float beatDistance = GetBeatLineDistance();
            float clickOnContentPos = localPosition.y;

            // 计算点击处相对于 Content 0 点（偏移值为判定线距离）的距离 -> 转换为多少个细分拍
            float relativeY = clickOnContentPos - judgeLineY;
            int subBeatIndex = Mathf.RoundToInt(relativeY / beatDistance * BeatAccuracy.CurrentValue);

            // 限制范围
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
