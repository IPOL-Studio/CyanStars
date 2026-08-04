// TODO: 待重构

#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class EditAreaNoteViewModel : BaseViewModel
    {
        private readonly BaseChartNoteData Data;
        private readonly float JudgeLineYOffset;
        private readonly bool useChartTracebackBeatOffset;

        public readonly ReadOnlyReactiveProperty<Vector2> AnchoredPosition;
        public readonly ReadOnlyReactiveProperty<float> HoldLength; // 仅 Hold 有效

        // 通过构造函数显式传递父级的 Model 和 CommandStack
        public EditAreaNoteViewModel(
            ChartEditorModel model,
            BaseChartNoteData data,
            EditAreaViewModel parentViewModel,
            float judgeLineYOffset,
            bool useChartTracebackBeatOffset = false)
            : base(model)
        {
            Data = data;
            JudgeLineYOffset = judgeLineYOffset;
            this.useChartTracebackBeatOffset = useChartTracebackBeatOffset;

            // 无论是缩放改变，还是当前 Note 数据改变，都重新获取当前的 Zoom 值并计算位置
            var dataChangedSignal = Model.SelectedNoteDataChangedSubject
                .Where(changedNote => changedNote == data)
                .Select(_ => Unit.Default);
            var updateSignal = Observable.Merge(
                    parentViewModel.BeatZoom.Select(_ => Unit.Default),
                    dataChangedSignal,
                    parentViewModel.ChartTracebackBeatOffset.Select(_ => Unit.Default)
                )
                .Select(_ => parentViewModel.BeatZoom.CurrentValue);

            // 当变化时，重新计算位置
            AnchoredPosition = updateSignal
                .Select(zoom => CalculatePosition(zoom))
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);

            // 如果是 Hold，需要根据缩放计算长度
            if (data is HoldChartNoteData holdData)
            {
                HoldLength = updateSignal
                    .Select(zoom => CalculateHoldLength(zoom, holdData))
                    .ToReadOnlyReactiveProperty()
                    .AddTo(Disposables);
            }
            else
            {
                HoldLength = Observable.Return(0f).ToReadOnlyReactiveProperty().AddTo(Disposables);
            }
        }

        private Vector2 CalculatePosition(double zoom)
        {
            double beatOffset = useChartTracebackBeatOffset ? Model.ChartTracebackBeatOffset.CurrentValue : 0;
            return EditAreaViewHelper.CalculateNoteAnchoredPosition(Data, JudgeLineYOffset, zoom, beatOffset);
        }

        private float CalculateHoldLength(double zoom, HoldChartNoteData holdData)
        {
            double beatInterval = EditAreaViewModel.DefaultMajorBeatLineInterval * zoom;
            double beatOffset = useChartTracebackBeatOffset ? Model.ChartTracebackBeatOffset.CurrentValue : 0;
            double startY = JudgeLineYOffset + ((holdData.JudgeBeat.ToDouble() - beatOffset) * beatInterval);
            double endY = JudgeLineYOffset + ((holdData.EndJudgeBeat.ToDouble() - beatOffset) * beatInterval);

            // 长度 = 结束位置 - 开始位置 - 头部微调
            return (float)Math.Max(0, endY - startY - 12.5f);
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
