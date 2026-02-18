#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateCurveFrameViewModel : BaseViewModel
    {
        private readonly SpeedTemplateViewModel SpeedTemplateViewModel;

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
