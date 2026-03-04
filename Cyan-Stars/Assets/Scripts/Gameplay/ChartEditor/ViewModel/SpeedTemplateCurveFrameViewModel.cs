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

        // 各个坐标点映射到 CurveFrame 时所用的偏移和缩放。
        // 先将数据坐标与 Offset 相加，然后得到的结果乘以 Scale，即可得到相对于 CurveFrame 局部坐标。
        // TODO: 改为动态拓展的视窗范围
        public const float DefaultViewportX = 1000f;
        public const float DefaultViewportY = 400f;
        public readonly ReactiveProperty<float> ScaleX = new ReactiveProperty<float>(1f);
        public readonly ReactiveProperty<float> ScaleY = new ReactiveProperty<float>(1f);
        public readonly ReactiveProperty<float> OffsetX = new ReactiveProperty<float>(0f);
        public readonly ReactiveProperty<float> OffsetY = new ReactiveProperty<float>(0f);

        /// <summary>
        /// 当前选中的变速模板
        /// </summary>
        public ReadOnlyReactiveProperty<SpeedTemplateDataEditorModel?> SelectedSpeedTemplateData =>
            SpeedTemplateViewModel.SelectedSpeedTemplateData;

        private readonly ReactiveProperty<ReadOnlyReactiveProperty<BezierPoint>?> selectedPoint = new ReactiveProperty<ReadOnlyReactiveProperty<BezierPoint>?>();

        /// <summary>
        /// 当前选中的贝塞尔点
        /// </summary>
        /// <remarks>外层 ReadOnlyReactiveProperty 用于在选择的点变化时发送通知
        /// 内层 ReadOnlyReactiveProperty 作为引用，防止在修改 BezierPoint 时重新销毁和生成点实例</remarks>
        public ReadOnlyReactiveProperty<ReadOnlyReactiveProperty<BezierPoint>?> SelectedPoint => selectedPoint;


        private readonly ReactiveProperty<ISynchronizedView<ReadOnlyReactiveProperty<BezierPoint>, SpeedTemplateBezierPointHandleItemViewModel>?> bezierPointViewModelsMap = new();

        public ReadOnlyReactiveProperty<ISynchronizedView<ReadOnlyReactiveProperty<BezierPoint>, SpeedTemplateBezierPointHandleItemViewModel>?> BezierPointViewModelsMap => bezierPointViewModelsMap;


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
                                .CreateView(pointWrapper =>
                                    new SpeedTemplateBezierPointHandleItemViewModel(Model, this, pointWrapper)
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

        public void SelectPoint(ReadOnlyReactiveProperty<BezierPoint>? bezierPointWrapper)
        {
            selectedPoint.Value = bezierPointWrapper;
        }
    }
}
