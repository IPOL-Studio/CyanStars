#nullable enable

using System;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateBezierPointHandleItemViewModel : BaseViewModel
    {
        private readonly SpeedTemplateCurveFrameViewModel SpeedTemplateCurveFrameViewModel;

        public readonly ReadOnlyReactiveProperty<BezierPoint> BezierPointWrapper;

        private readonly ReactiveProperty<bool> selfSelected = new ReactiveProperty<bool>(false);
        public ReadOnlyReactiveProperty<bool> SelfSelected => selfSelected;


        public ReadOnlyReactiveProperty<float> ScaleX => SpeedTemplateCurveFrameViewModel.ScaleX;
        public ReadOnlyReactiveProperty<float> ScaleY => SpeedTemplateCurveFrameViewModel.ScaleY;
        public ReadOnlyReactiveProperty<float> OffsetX => SpeedTemplateCurveFrameViewModel.OffsetX;
        public ReadOnlyReactiveProperty<float> OffsetY => SpeedTemplateCurveFrameViewModel.OffsetY;


        /// <summary>
        /// 贝塞尔点 VM 构造函数
        /// 当选中的变速模板变化时，会销毁并释放所有旧 VM 并构建新 VM。
        /// 新增/删除贝塞尔点也会相应构建和销毁 VM，但更新贝塞尔点位置时不会变化。
        /// </summary>
        public SpeedTemplateBezierPointHandleItemViewModel(
            ChartEditorModel model,
            SpeedTemplateCurveFrameViewModel speedTemplateCurveFrameViewModel,
            ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper
        )
            : base(model)
        {
            SpeedTemplateCurveFrameViewModel = speedTemplateCurveFrameViewModel;
            BezierPointWrapper = bezierPointWrapper;

            SpeedTemplateCurveFrameViewModel.SelectedPoint
                .Subscribe(selectedPoint => selfSelected.Value = selectedPoint == BezierPointWrapper)
                .AddTo(base.Disposables);
        }


        public void SelectPoint()
        {
            if (SpeedTemplateCurveFrameViewModel.SelectedPoint.CurrentValue == BezierPointWrapper)
                return;

            SpeedTemplateCurveFrameViewModel.SelectPoint(BezierPointWrapper);
        }

        public void SetSubPointPos(Vector2 localPoint, BezierPointSubItemType type)
        {
            // 将本地坐标转换到贝塞尔点数据位置
            int msTime = (int)(localPoint.x / SpeedTemplateCurveFrameViewModel.ScaleX.CurrentValue - SpeedTemplateCurveFrameViewModel.OffsetX.CurrentValue);
            float value = localPoint.y / SpeedTemplateCurveFrameViewModel.ScaleY.CurrentValue - SpeedTemplateCurveFrameViewModel.OffsetY.CurrentValue;

            // TODO: 做个验证避免错误数据抛异常
            BezierPoint bezierPoint;
            switch (type)
            {
                case BezierPointSubItemType.PosPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(msTime, value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.LeftControlPoint.MsTime, BezierPointWrapper.CurrentValue.LeftControlPoint.Value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.RightControlPoint.MsTime, BezierPointWrapper.CurrentValue.RightControlPoint.Value)
                    );
                    break;
                case BezierPointSubItemType.LeftControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(BezierPointWrapper.CurrentValue.PositionPoint.MsTime, BezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos(msTime, value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.RightControlPoint.MsTime, BezierPointWrapper.CurrentValue.RightControlPoint.Value)
                    );
                    break;
                case BezierPointSubItemType.RightControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(BezierPointWrapper.CurrentValue.PositionPoint.MsTime, BezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.LeftControlPoint.MsTime, BezierPointWrapper.CurrentValue.LeftControlPoint.Value),
                        new BezierPointPos(msTime, value)
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                BezierPointWrapper,
                bezierPoint
            );
        }
    }
}
