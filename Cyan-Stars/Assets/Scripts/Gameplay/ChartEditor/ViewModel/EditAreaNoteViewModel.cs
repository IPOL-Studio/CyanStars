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
        private readonly BaseChartNoteData data;
        private readonly EditAreaViewModel parentViewModel;
        private readonly float judgeLineYOffset;

        public readonly ReadOnlyReactiveProperty<Vector2> AnchoredPosition;
        public readonly ReadOnlyReactiveProperty<float> HoldLength; // 仅 Hold 有效

        private const float NotePosScale = 802.5f;
        private const float NotePosOffset = -321f;
        private const float BreakLeftX = -468.8f;
        private const float BreakRightX = 468.8f;

        // 通过构造函数显式传递父级的 Model 和 CommandStack
        public EditAreaNoteViewModel(
            ChartEditorModel model,
            BaseChartNoteData data,
            EditAreaViewModel parentViewModel,
            float judgeLineYOffset)
            : base(model)
        {
            this.data = data;
            this.parentViewModel = parentViewModel;
            this.judgeLineYOffset = judgeLineYOffset;

            // 无论是缩放改变，还是当前 Note 数据改变，都重新获取当前的 Zoom 值并计算位置
            var dataChangedSignal = Model.SelectedNoteDataChangedSubject
                .Where(changedNote => changedNote == data)
                .Select(_ => Unit.Default);
            var updateSignal = Observable.Merge(
                    parentViewModel.BeatZoom.Select(_ => Unit.Default),
                    dataChangedSignal
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
            // 计算 X 轴
            float xPos = 0;
            switch (data.Type)
            {
                case NoteType.Tap:
                case NoteType.Drag:
                case NoteType.Click:
                case NoteType.Hold:
                    if (data is IChartNoteNormalPos normalNote)
                    {
                        xPos = normalNote.Pos * NotePosScale + NotePosOffset;
                    }

                    break;
                case NoteType.Break:
                    if (data is BreakChartNoteData breakNote)
                    {
                        xPos = breakNote.BreakNotePos == BreakNotePos.Left ? BreakLeftX : BreakRightX;
                    }

                    break;
            }

            // 计算 Y 轴 (JudgeLineOffset + Beat * Interval * Zoom)
            // DefaultMajorBeatLineInterval * Zoom 即为每拍的像素距离
            double beatInterval = EditAreaViewModel.DefaultMajorBeatLineInterval * zoom;
            double yPos = judgeLineYOffset + (data.JudgeBeat.ToDouble() * beatInterval);

            return new Vector2(xPos, (float)yPos);
        }

        private float CalculateHoldLength(double zoom, HoldChartNoteData holdData)
        {
            double beatInterval = EditAreaViewModel.DefaultMajorBeatLineInterval * zoom;
            double startY = judgeLineYOffset + (holdData.JudgeBeat.ToDouble() * beatInterval);
            double endY = judgeLineYOffset + (holdData.EndJudgeBeat.ToDouble() * beatInterval);

            // 长度 = 结束位置 - 开始位置 - 头部微调
            return (float)Math.Max(0, endY - startY - 12.5f);
        }

        public void OnLeftClick()
        {
            if (Model.SelectedEditTool.CurrentValue == EditToolType.Eraser)
            {
                if (Model.SelectedNoteData.Value == data)
                {
                    Model.SelectedNoteData.Value = null;
                }

                CommandStack.ExecuteCommand(
                    () => Model.ChartData.CurrentValue.Notes.Remove(data),
                    () => NoteListHelper.TryInsertItem(Model.ChartData.CurrentValue.Notes, data)
                );
            }
            else
            {
                if (Model.SelectedNoteData.Value != data)
                {
                    Model.SelectedNoteData.Value = data;
                }
            }
        }

        public void OnRightClick()
        {
            if (Model.SelectedNoteData.Value == data)
            {
                Model.SelectedNoteData.Value = null;
            }

            CommandStack.ExecuteCommand(
                () => Model.ChartData.CurrentValue.Notes.Remove(data),
                () => NoteListHelper.TryInsertItem(Model.ChartData.CurrentValue.Notes, data)
            );
        }
    }
}
