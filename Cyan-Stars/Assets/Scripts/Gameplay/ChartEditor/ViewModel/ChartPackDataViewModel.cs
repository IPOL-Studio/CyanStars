#nullable enable

using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
        // TODO: 谱包导出还没做
        private Vector2? dragStartCropPos;
        private float? dragStartCropHeight;

        public readonly ReadOnlyReactiveProperty<bool> CanvasVisible;
        public readonly ReadOnlyReactiveProperty<string> ChartPackTitle;

        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField3String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField3String;

        public readonly ReadOnlyReactiveProperty<string> CoverFilePathString;
        public readonly ReadOnlyReactiveProperty<bool> CoverCropAreaVisible;


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
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);

            CoverFilePathString = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Select(path => path ?? "")
                .ToReadOnlyReactiveProperty("")
                .AddTo(base.Disposables);
            CoverCropAreaVisible = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Select(path => path != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
        }

        public void CloseCanvas()
        {
            if (!Model.ChartPackDataCanvasVisibility.Value)
                return;

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackDataCanvasVisibility.Value = false,
                    () => Model.ChartPackDataCanvasVisibility.Value = true
                )
            );
        }

        public void SetChartPackTitle(string newTitle)
        {
            string oldTitle = Model.ChartPackData.CurrentValue.Title.Value;
            if (newTitle == oldTitle)
                return;

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.Title.Value = newTitle,
                    () => Model.ChartPackData.CurrentValue.Title.Value = oldTitle
                )
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

            if (newBeat > Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = newBeat,
                    () => Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = oldBeat
                )
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

            if (newBeat < Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () => Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = newBeat,
                    () => Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = oldBeat
                )
            );
        }

        /// <summary>
        /// 开始拖拽，记录初始状态
        /// </summary>
        public void OnCoverCropDragBegin()
        {
            dragStartCropPos = Model.ChartPackData.CurrentValue.CropStartPosition.Value;
            dragStartCropHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;
        }

        /// <summary>
        /// 拖拽结束，提交命令
        /// </summary>
        public void OnCoverCropDragEnd()
        {
            var finalPos = Model.ChartPackData.CurrentValue.CropStartPosition.Value;
            var finalHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;

            var oldPos = dragStartCropPos;
            var oldHeight = dragStartCropHeight;

            if (oldPos != null && oldHeight != null &&
                (Vector2)oldPos == finalPos &&
                Mathf.Approximately((float)oldHeight, finalHeight ?? 0))
            {
                return;
            }

            CommandManager.ExecuteCommand(
                new DelegateCommand(
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.CropStartPosition.Value = finalPos;
                        Model.ChartPackData.CurrentValue.CropHeight.Value = finalHeight;
                    },
                    () =>
                    {
                        Model.ChartPackData.CurrentValue.CropStartPosition.Value = oldPos;
                        Model.ChartPackData.CurrentValue.CropHeight.Value = oldHeight;
                    }
                )
            );
        }
    }
}
