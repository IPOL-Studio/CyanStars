#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateBezierPointHandleItemViewModel : BaseViewModel
    {
        private readonly SpeedTemplateCurveFrameViewModel SpeedTemplateCurveFrameViewModel;

        public readonly ReadOnlyReactiveProperty<BezierPointWrapperModel> BezierPoint;

        private readonly ReactiveProperty<bool> selfSelected = new ReactiveProperty<bool>(false);
        public ReadOnlyReactiveProperty<bool> SelfSelected => selfSelected;


        public SpeedTemplateBezierPointHandleItemViewModel(
            ChartEditorModel model,
            SpeedTemplateCurveFrameViewModel speedTemplateCurveFrameViewModel,
            BezierPointWrapperModel bezierPoint
        )
            : base(model)
        {
            SpeedTemplateCurveFrameViewModel = speedTemplateCurveFrameViewModel;
            BezierPoint = new ReactiveProperty<BezierPointWrapperModel>(bezierPoint);

            SpeedTemplateCurveFrameViewModel.SelectedPoint
                .Subscribe(selectedPoint => selfSelected.Value = selectedPoint == BezierPoint.CurrentValue)
                .AddTo(base.Disposables);
        }
    }
}
