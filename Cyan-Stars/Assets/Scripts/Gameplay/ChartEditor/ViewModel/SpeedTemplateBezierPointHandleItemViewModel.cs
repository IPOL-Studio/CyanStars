#nullable enable

using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateBezierPointHandleItemViewModel : BaseViewModel
    {
        private readonly SpeedTemplateCurveFrameViewModel SpeedTemplateCurveFrameViewModel;

        public readonly BezierPointWrapperModel BezierPoint; // 结构体，在更新时直接销毁 VM 并重新创建
        public readonly ReactiveProperty<bool> SelfSelected = new ReactiveProperty<bool>();


        public SpeedTemplateBezierPointHandleItemViewModel(
            ChartEditorModel model,
            SpeedTemplateCurveFrameViewModel speedTemplateCurveFrameViewModel,
            BezierPointWrapperModel bezierPoint
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
