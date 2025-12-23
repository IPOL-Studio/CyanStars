#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
        // TODO: 曲绘导入、裁剪，谱包导出还没做

        // VM 私有属性
        private readonly BindableProperty<bool> canvasVisible;
        private readonly BindableProperty<string> chartPackTitle;
        private readonly BindableProperty<string> previewStartBeatField1String;
        private readonly BindableProperty<string> previewStartBeatField2String;
        private readonly BindableProperty<string> previewStartBeatField3String;
        private readonly BindableProperty<string> previewEndBeatField1String;
        private readonly BindableProperty<string> previewEndBeatField2String;
        private readonly BindableProperty<string> previewEndBeatField3String;

        // 暴露给 V 的只读属性
        public IReadonlyBindableProperty<bool> CanvasVisible => canvasVisible;
        public IReadonlyBindableProperty<string> ChartPackTitle => chartPackTitle;
        public IReadonlyBindableProperty<string> PreviewStartBeatField1String => previewStartBeatField1String;
        public IReadonlyBindableProperty<string> PreviewStartBeatField2String => previewStartBeatField2String;
        public IReadonlyBindableProperty<string> PreviewStartBeatField3String => previewStartBeatField3String;
        public IReadonlyBindableProperty<string> PreviewEndBeatField1String => previewEndBeatField1String;
        public IReadonlyBindableProperty<string> PreviewEndBeatField2String => previewEndBeatField2String;
        public IReadonlyBindableProperty<string> PreviewEndBeatField3String => previewEndBeatField3String;


        public ChartPackDataViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            // 初始化本 VM 属性值
            canvasVisible =
                new BindableProperty<bool>(Model.ChartPackDataCanvasVisibility.Value);
            chartPackTitle =
                new BindableProperty<string>(Model.ChartPackData.Title);
            previewStartBeatField1String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewStartBeat.IntegerPart.ToString());
            previewStartBeatField2String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewStartBeat.Numerator.ToString());
            previewStartBeatField3String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewStartBeat.Denominator.ToString());
            previewEndBeatField1String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewEndBeat.IntegerPart.ToString());
            previewEndBeatField2String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewEndBeat.Numerator.ToString());
            previewEndBeatField3String =
                new BindableProperty<string>(Model.ChartPackData.MusicPreviewEndBeat.Denominator.ToString());

            // M->VM 订阅
            Model.ChartPackDataCanvasVisibility.OnValueChanged += isOn =>
            {
                canvasVisible.Value = isOn;
            };
            Model.OnChartPackBasicDataChanged += data =>
            {
                chartPackTitle.Value = data.Title;
                previewStartBeatField1String.Value = data.MusicPreviewStartBeat.IntegerPart.ToString();
                previewStartBeatField2String.Value = data.MusicPreviewStartBeat.Numerator.ToString();
                previewStartBeatField3String.Value = data.MusicPreviewStartBeat.Denominator.ToString();
                previewEndBeatField1String.Value = data.MusicPreviewEndBeat.IntegerPart.ToString();
                previewEndBeatField2String.Value = data.MusicPreviewEndBeat.Numerator.ToString();
                previewEndBeatField3String.Value = data.MusicPreviewEndBeat.Denominator.ToString();
            };
        }


        // V->VM

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
            string oldTitle = Model.ChartPackData.Title;
            if (newTitle == oldTitle)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartPackTitle(newTitle);
                }, () =>
                {
                    Model.SetChartPackTitle(oldTitle);
                })
            );
        }

        public void SetPreviewStartBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                previewStartBeatField1String.ForceNotify();
                previewStartBeatField2String.ForceNotify();
                previewStartBeatField3String.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                previewStartBeatField1String.ForceNotify();
                previewStartBeatField2String.ForceNotify();
                previewStartBeatField3String.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.MusicPreviewStartBeat;

            if (newBeat == oldBeat)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartPackPreviewStartBeat(newBeat);
                }, () =>
                {
                    Model.SetChartPackPreviewStartBeat(oldBeat);
                })
            );
        }

        public void SetPreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                previewEndBeatField1String.ForceNotify();
                previewEndBeatField2String.ForceNotify();
                previewEndBeatField3String.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                previewEndBeatField1String.ForceNotify();
                previewEndBeatField2String.ForceNotify();
                previewEndBeatField3String.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.MusicPreviewEndBeat;

            if (newBeat == oldBeat)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartPackPreviewEndBeat(newBeat);
                }, () =>
                {
                    Model.SetChartPackPreviewEndBeat(oldBeat);
                })
            );
        }
    }
}
