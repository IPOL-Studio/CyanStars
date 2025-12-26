#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
        // TODO: 曲绘导入、裁剪，谱包导出还没做

        public readonly ReadOnlyReactiveProperty<bool> CanvasVisible;
        public readonly ReadOnlyReactiveProperty<string> ChartPackTitle;

        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField3String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField3String;

        public readonly ReadOnlyReactiveProperty<string?> CoverFilePathString;


        public ChartPackDataViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            CanvasVisible = Model.ChartPackDataCanvasVisibility
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            ChartPackTitle = Model.ChartPackData
                .Select(data => data.Title.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.Title.Value)
                .AddTo(base.Disposables);

            PreviewStartBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.IntegerPart.ToString())
                .AddTo(base.Disposables);
            PreviewStartBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Numerator.ToString())
                .AddTo(base.Disposables);
            PreviewStartBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Denominator.ToString())
                .AddTo(base.Disposables);
            PreviewEndBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.IntegerPart.ToString())
                .AddTo(base.Disposables);
            PreviewEndBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Numerator.ToString())
                .AddTo(base.Disposables);
            PreviewEndBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Denominator.ToString())
                .AddTo(base.Disposables);

            CoverFilePathString = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CoverFilePath.Value)
                .AddTo(base.Disposables);
        }

        public void CloseCanvas()
        {
            if (!Model.ChartPackDataCanvasVisibility.Value)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackDataCanvasVisibility.Value = false;
                }, () =>
                {
                    Model.ChartPackDataCanvasVisibility.Value = true;
                })
            );
        }

        public void SetChartPackTitle(string newTitle)
        {
            string oldTitle = Model.ChartPackData.CurrentValue.Title.Value;
            if (newTitle == oldTitle)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.Title.Value = newTitle;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.Title.Value = oldTitle;
                })
            );
        }

        public void SetPreviewStartBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = newBeat;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = oldBeat;
                })
            );
        }

        public void SetPreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = newBeat;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = oldBeat;
                })
            );
        }
    }
}
