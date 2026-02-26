#nullable enable

using CyanStars.Chart;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateCurveFrameViewModel : BaseViewModel
    {
        private readonly SpeedTemplateViewModel SpeedTemplateViewModel;

        /// <summary>
        /// 当前选中的变速模板
        /// </summary>
        public ReadOnlyReactiveProperty<SpeedTemplateDataEditorModel?> SelectedSpeedTemplateData =>
            SpeedTemplateViewModel.SelectedSpeedTemplateData;

        private readonly ReactiveProperty<BezierPoint?> selectedPoint = new ReactiveProperty<BezierPoint?>();

        /// <summary>
        /// 当前选中的贝塞尔点
        /// </summary>
        public ReadOnlyReactiveProperty<BezierPoint?> SelectedPoint => selectedPoint;


        public SpeedTemplateCurveFrameViewModel(
            ChartEditorModel model,
            SpeedTemplateViewModel speedTemplateViewModel
        )
            : base(model)
        {
            SpeedTemplateViewModel = speedTemplateViewModel;
        }
    }
}
