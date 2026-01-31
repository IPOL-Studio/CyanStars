#nullable enable

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
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> JudgeBeatField2Text =>
            Model.SelectedNoteData
                .Select(note => note == null ? "" : note.JudgeBeat.Numerator.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> JudgeBeatField3Text =>
            Model.SelectedNoteData
                .Select(note => note == null ? "" : note.JudgeBeat.Denominator.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField1Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField2Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.Numerator.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> EndJudgeBeatField3Text =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type != NoteType.Hold) ? "" : (note as HoldChartNoteData).EndJudgeBeat.Denominator.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);

        public ReadOnlyReactiveProperty<string> PosFieldText =>
            Model.SelectedNoteData
                .Select(note => (note == null || note.Type == NoteType.Break) ? "" : (note as IChartNoteNormalPos).Pos.ToString())
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);


        public NoteAttributeViewModel(ChartEditorModel model)
            : base(model)
        {
        }
    }
}
