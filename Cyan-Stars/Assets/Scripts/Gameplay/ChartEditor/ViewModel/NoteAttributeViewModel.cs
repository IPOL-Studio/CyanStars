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
        // Frame 可见性
        public ReadOnlyReactiveProperty<bool> FrameVisibility =>
            Model.SelectedNoteData
                .Select(note => note != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<bool> JudgeBeatFrameVisibility => new BindableReactiveProperty<bool>(true);

        public ReadOnlyReactiveProperty<bool> EndJudgeBeatFrameVisibility =>
            Model.SelectedNoteData
                .Select(note => note != null && note.Type == NoteType.Hold)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<bool> PosFrameVisibility =>
            Model.SelectedNoteData
                .Select(note => note != null && note.Type != NoteType.Break)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<bool> BreakPosFrameVisibility =>
            PosFrameVisibility
                .Select(visibility => !visibility)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<bool> CorrectAudioFrameVisibility => new ReactiveProperty<bool>(false); //TODO
        public ReadOnlyReactiveProperty<bool> HitAudioFrameVisibility => new BindableReactiveProperty<bool>(false); // TODO
        public ReadOnlyReactiveProperty<bool> SpeedTemplateFrameVisibility => new BindableReactiveProperty<bool>(false); //TODO
        public ReadOnlyReactiveProperty<bool> SpeedOffsetFrameVisibility => new BindableReactiveProperty<bool>(false); // TODO

        // 元素值
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

        public ReadOnlyReactiveProperty<bool> BreakLeftPosState =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Break) ? false : ((note as BreakChartNoteData).BreakNotePos == BreakNotePos.Left))
                .ToReadOnlyReactiveProperty(false)
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<bool> BreakRightPosState =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Break) ? false : ((note as BreakChartNoteData).BreakNotePos == BreakNotePos.Right))
                .ToReadOnlyReactiveProperty(false)
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

            if (!Beat.TryCreateBeat(integerPartInt, numeratorInt, denominatorInt, out var beat))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            Model.SelectedNoteData.CurrentValue.JudgeBeat = beat;
            Model.SelectedNoteDataChangedSubject.OnNext(Model.SelectedNoteData.CurrentValue);
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

            if (!Beat.TryCreateBeat(integerPartInt, numeratorInt, denominatorInt, out var beat))
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            (Model.SelectedNoteData.CurrentValue as HoldChartNoteData).EndJudgeBeat = beat;
            Model.SelectedNoteDataChangedSubject.OnNext(Model.SelectedNoteData.CurrentValue);
        }

        public void UpdateNotePos(string pos)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (Model.SelectedNoteData.CurrentValue.Type == NoteType.Break)
                throw new Exception("SelectedNoteData is break");

            if (!float.TryParse(pos, out var posFloat) || posFloat < 0f || 0.8f < posFloat)
            {
                Model.SelectedNoteData.ForceNotify();
                return;
            }

            (Model.SelectedNoteData.CurrentValue as IChartNoteNormalPos).Pos = posFloat;
            Model.SelectedNoteDataChangedSubject.OnNext(Model.SelectedNoteData.CurrentValue);
        }

        public void UpdateBreakNotePos(BreakNotePos pos)
        {
            if (Model.SelectedNoteData.CurrentValue == null)
                throw new Exception("SelectedNoteData is null");

            if (Model.SelectedNoteData.CurrentValue.Type != NoteType.Break)
                throw new Exception("SelectedNoteData is not break");

            (Model.SelectedNoteData.CurrentValue as BreakChartNoteData).BreakNotePos = pos;
            Model.SelectedNoteDataChangedSubject.OnNext(Model.SelectedNoteData.CurrentValue);
        }
    }
}
