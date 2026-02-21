#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils.SpeedTemplate;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateCurveFrameViewModel : BaseViewModel
    {
        private readonly SpeedTemplateViewModel SpeedTemplateViewModel;

        public Subject<SpeedTemplateData> SelectedSpeedTemplateDataPropertyUpdatedSubject =>
            SpeedTemplateViewModel.SelectedSpeedTemplateDataPropertyUpdatedSubject;

        /// <summary>
        /// 选中的变速模板中的贝塞尔曲线数据发生了变化
        /// </summary>
        public Subject<BezierCurves> SelectedBezierCurvePropertyUpdatedSubject = new();


        /// <summary>
        /// 当前选中的变速模板
        /// </summary>
        public ReadOnlyReactiveProperty<SpeedTemplateData?> SelectedSpeedTemplateData =>
            SpeedTemplateViewModel.SelectedSpeedTemplateData;


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
