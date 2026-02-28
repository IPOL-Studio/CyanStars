#nullable enable

using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateBezierPointHandleItemViewModel : BaseViewModel
    {
        private readonly SpeedTemplateCurveFrameViewModel SpeedTemplateCurveFrameViewModel;
        private readonly BezierPoint BezierPoint;

        public readonly ReactiveProperty<bool> SelfSelected = new ReactiveProperty<bool>();


        public SpeedTemplateBezierPointHandleItemViewModel(
            ChartEditorModel model,
            SpeedTemplateCurveFrameViewModel speedTemplateCurveFrameViewModel,
            BezierPoint bezierPoint
        )
            : base(model)
        {
            SpeedTemplateCurveFrameViewModel = speedTemplateCurveFrameViewModel;
            BezierPoint = bezierPoint;

            SpeedTemplateCurveFrameViewModel.SelectedPoint
                .Subscribe(selectedPoint => SelfSelected.Value = selectedPoint == BezierPoint)
                .AddTo(base.Disposables);
        }
    }
}
