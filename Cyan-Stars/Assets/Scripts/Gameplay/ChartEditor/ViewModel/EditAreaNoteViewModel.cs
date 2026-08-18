#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditAreaNoteViewModel : BaseViewModel
    {
        private readonly bool useChartTracebackBeatOffset;

        /// <summary>
        /// 音符数据
        /// </summary>
        public BaseChartNoteData Data { get; }

        /// <summary>
        /// 已应用谱面回溯偏移的判定拍
        /// </summary>
        public readonly ReadOnlyReactiveProperty<double> PositionBeat;

        /// <summary>
        /// 已应用谱面回溯偏移的结束拍（仅 Hold 有效；其余音符与 PositionBeat 相同）
        /// </summary>
        public readonly ReadOnlyReactiveProperty<double> PositionEndBeat;

        /// <summary>
        /// 布局无关的位置信息发生变化时触发
        /// </summary>
        public readonly Observable<Unit> PositionChanged;

        // 通过构造函数显式传递父级的 Model 和 CommandStack
        public EditAreaNoteViewModel(
            ChartEditorModel model,
            BaseChartNoteData data,
            bool useChartTracebackBeatOffset = false)
            : base(model)
        {
            Data = data;
            this.useChartTracebackBeatOffset = useChartTracebackBeatOffset;

            // 选中音符的 Pos/BreakPos/JudgeBeat/EndJudgeBeat 变化时，重新计算位置
            var dataChangedSignal = Model.SelectedNoteDataChangedSubject
                .Where(changedNote => changedNote == data)
                .Select(_ => Unit.Default);

            // 初始值 + 音符数据变化 + 谱面回溯偏移变化。
            // BeatZoom 等 view 布局变化不在此处处理，由 View 自行订阅。
            PositionChanged = (useChartTracebackBeatOffset
                    ? Observable.Merge(dataChangedSignal, Model.ChartTracebackBeatOffset.Select(_ => Unit.Default))
                    : dataChangedSignal)
                .Prepend(Unit.Default);

            PositionBeat = PositionChanged
                .Select(_ => GetPositionBeat())
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);

            PositionEndBeat = PositionChanged
                .Select(_ => GetPositionEndBeat())
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);
        }

        private double GetBeatOffset()
        {
            return useChartTracebackBeatOffset ? Model.ChartTracebackBeatOffset.CurrentValue : 0;
        }

        private double GetPositionBeat()
        {
            return Data.JudgeBeat.ToDouble() - GetBeatOffset();
        }

        private double GetPositionEndBeat()
        {
            double endBeat = Data is HoldChartNoteData holdData
                ? holdData.EndJudgeBeat.ToDouble()
                : Data.JudgeBeat.ToDouble();

            return endBeat - GetBeatOffset();
        }

        public void OnLeftKeyDown()
        {
            if (Model.SelectedEditTool.CurrentValue == EditToolType.Eraser)
            {
                if (Model.SelectedNoteData.Value == Data)
                {
                    Model.SelectedNoteData.Value = null;
                }

                CommandStack.ExecuteCommand(
                    () => Model.ChartData.CurrentValue.Notes.Remove(Data),
                    () => NoteListHelper.TryInsertItem(Model.ChartData.CurrentValue.Notes, Data)
                );
            }
            else
            {
                if (Model.SelectedNoteData.Value != Data)
                {
                    Model.SelectedNoteData.Value = Data;
                }
            }
        }

        public void OnRightKeyDown()
        {
            if (Model.SelectedNoteData.Value == Data)
            {
                Model.SelectedNoteData.Value = null;
            }

            CommandStack.ExecuteCommand(
                () => Model.ChartData.CurrentValue.Notes.Remove(Data),
                () => NoteListHelper.TryInsertItem(Model.ChartData.CurrentValue.Notes, Data)
            );
        }
    }
}
