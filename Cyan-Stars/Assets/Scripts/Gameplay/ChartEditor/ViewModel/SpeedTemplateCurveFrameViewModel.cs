#nullable enable

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

        private readonly ReactiveProperty<BezierPointWrapperModel?> selectedPoint = new ReactiveProperty<BezierPointWrapperModel?>();

        /// <summary>
        /// 当前选中的贝塞尔点
        /// </summary>
        public ReadOnlyReactiveProperty<BezierPointWrapperModel?> SelectedPoint => selectedPoint;


        private readonly ReactiveProperty<ISynchronizedView<BezierPointWrapperModel, SpeedTemplateBezierPointHandleItemViewModel>?> bezierPointViewModelsMap = new();

        public ReadOnlyReactiveProperty<ISynchronizedView<BezierPointWrapperModel, SpeedTemplateBezierPointHandleItemViewModel>?> BezierPointViewModelsMap => bezierPointViewModelsMap;


        public SpeedTemplateCurveFrameViewModel(
            ChartEditorModel model,
            SpeedTemplateViewModel speedTemplateViewModel
        )
            : base(model)
        {
            SpeedTemplateViewModel = speedTemplateViewModel;

            // 变速模板变化时清空选中的贝塞尔点
            SelectedSpeedTemplateData
                .Subscribe(_ => selectedPoint.Value = null)
                .AddTo(base.Disposables);

            // 变速模板变化时重新构建子 VM 和 V
            SelectedSpeedTemplateData
                .Subscribe(selectedData =>
                    {
                        bezierPointViewModelsMap.CurrentValue?.Dispose();

                        if (selectedData != null)
                        {
                            bezierPointViewModelsMap.Value = selectedData.BezierCurves.Points
                                .CreateView(point =>
                                    new SpeedTemplateBezierPointHandleItemViewModel(Model, this, point)
                                );
                        }
                        else
                        {
                            bezierPointViewModelsMap.Value = null;
                        }
                    }
                )
                .AddTo(base.Disposables);

            // 确保 ViewModel 销毁时，最后一个 View 也能被正确释放
            base.Disposables.Add(Disposable.Create(() => bezierPointViewModelsMap.CurrentValue?.Dispose()));
        }
    }
}
