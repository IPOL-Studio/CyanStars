#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class NoteAttributeViewModel : BaseViewModel
    {
        public ReadOnlyReactiveProperty<BaseChartNoteData?> SelectedNoteData => Model.SelectedNoteData;

        // 元素值 // TODO: 让 view 自己解析
        public ReadOnlyReactiveProperty<string> JudgeBeatField1Text =>
            Model.SelectedNoteData
                .Select(note => note == null ? "" : note.JudgeBeat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> JudgeBeatField2Text =>
            Model.SelectedNoteData
                .Select(note => note == null ? "" : note.JudgeBeat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> JudgeBeatField3Text =>
            Model.SelectedNoteData
                .Select(note => note == null ? "" : note.JudgeBeat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField1Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField2Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField3Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> PosFieldText =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type == NoteType.Break) ? "" : (note as IChartNoteNormalPos).Pos.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<BreakNotePos?> BreakNotePos =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Break) ? (BreakNotePos?)null : (note as BreakChartNoteData).BreakNotePos)
                .ToReadOnlyReactiveProperty(null)
                .AddTo(base.Disposables);


        public NoteAttributeViewModel(ChartEditorModel model)
            : base(model)
        {
        }

        public void UpdateNoteJudgeBeat(string integerPart, string numerator, string denominator)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (!int.TryParse(integerPart, out var integerPartInt) ||
                !int.TryParse(numerator, out var numeratorInt) ||
                !int.TryParse(denominator, out var denominatorInt))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPartInt, numeratorInt, denominatorInt, out var newJudgeBeat))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            if (Model.SelectedNoteData.CurrentValue.Type == NoteType.Hold &&
                ((HoldChartNoteData)Model.SelectedNoteData.CurrentValue).EndJudgeBeat < newJudgeBeat)
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            Beat oldJudgeBeat = Model.SelectedNoteData.CurrentValue.JudgeBeat;
            var note = Model.SelectedNoteData.CurrentValue;
            CommandStack.ExecuteCommand(
                () =>
                {
                    note.JudgeBeat = newJudgeBeat;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                },
                () =>
                {
                    note.JudgeBeat = oldJudgeBeat;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                }
            );
        }

        public void UpdateNoteEndJudgeBeat(string integerPart, string numerator, string denominator)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (Model.SelectedNoteData.CurrentValue.Type != NoteType.Hold)
                throw new Exception("SelectedNoteData is not hold");

            if (!int.TryParse(integerPart, out var integerPartInt) ||
                !int.TryParse(numerator, out var numeratorInt) ||
                !int.TryParse(denominator, out var denominatorInt))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPartInt, numeratorInt, denominatorInt, out var newEndBeat))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            HoldChartNoteData note = (HoldChartNoteData)Model.SelectedNoteData.CurrentValue;

            if (newEndBeat < note.JudgeBeat)
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            Beat oldEndBeat = note.EndJudgeBeat;
            CommandStack.ExecuteCommand(
                () =>
                {
                    note.EndJudgeBeat = newEndBeat;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                },
                () =>
                {
                    note.EndJudgeBeat = oldEndBeat;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                }
            );
        }

        public void UpdateNotePos(string pos)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (Model.SelectedNoteData.CurrentValue.Type == NoteType.Break)
                throw new Exception("SelectedNoteData is break");

            if (!float.TryParse(pos, out var newPosFloat) || newPosFloat < 0f || 0.8f < newPosFloat)
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            float oldPosFloat = ((IChartNoteNormalPos)Model.SelectedNoteData.CurrentValue).Pos;
            var note = (IChartNoteNormalPos)Model.SelectedNoteData.CurrentValue;
            CommandStack.ExecuteCommand(
                () =>
                {
                    note.Pos = newPosFloat;
                    Model.SelectedNoteDataChangedSubject.OnNext((BaseChartNoteData)note);
                },
                () =>
                {
                    note.Pos = oldPosFloat;
                    Model.SelectedNoteDataChangedSubject.OnNext((BaseChartNoteData)note);
                }
            );
        }

        public void UpdateBreakNotePos(BreakNotePos newBreakPos)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (Model.SelectedNoteData.CurrentValue.Type != NoteType.Break)
                throw new Exception("SelectedNoteData is not break");

            BreakNotePos oldBreakPos = ((BreakChartNoteData)Model.SelectedNoteData.CurrentValue).BreakNotePos;
            var note = (BreakChartNoteData)Model.SelectedNoteData.CurrentValue;
            CommandStack.ExecuteCommand(
                () =>
                {
                    note.BreakNotePos = newBreakPos;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                },
                () =>
                {
                    note.BreakNotePos = oldBreakPos;
                    Model.SelectedNoteDataChangedSubject.OnNext(note);
                }
            );
        }
    }
}
