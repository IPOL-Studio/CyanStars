#nullable enable

using System;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Command;
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


        // 控制点可用状态，首个贝塞尔点永远禁用左侧控制点，末个贝塞尔点永远禁用右侧控制点
        public readonly bool IsFirstPoint;
        public readonly bool IsLastPoint;


        private BezierPoint? recordedLocalPoint = null;
        private BezierPointSubItemType? recordedType = null!;


        /// <summary>
        /// 贝塞尔点 VM 构造函数
        /// 当选中的变速模板变化时，会销毁并释放所有旧 VM 并构建新 VM。
        /// 新增/删除贝塞尔点也会相应构建和销毁 VM，但更新贝塞尔点位置时不会变化。
        /// </summary>
        public SpeedTemplateBezierPointHandleItemViewModel(
            ChartEditorModel model,
            SpeedTemplateCurveFrameViewModel speedTemplateCurveFrameViewModel,
            ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper,
            bool isFirstPoint,
            bool isLastPoint
        )
            : base(model)
        {
            SpeedTemplateCurveFrameViewModel = speedTemplateCurveFrameViewModel;
            BezierPointWrapper = bezierPointWrapper;
            IsFirstPoint = isFirstPoint;
            IsLastPoint = isLastPoint;

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
            if (recordedLocalPoint == null || recordedType == null)
                throw new Exception("记录的开始拖拽点位置或类型为空！");
            if (recordedType != type)
                throw new Exception("拖拽时的 type 与开始拖拽时记录的不一致，请检查！");

            GetPointDataByLocalPoint(localPoint, type, out BezierPoint bezierPoint);
            SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                BezierPointWrapper,
                bezierPoint
            );
        }

        public void RecordSubPointPos(BezierPointSubItemType draggingHandleType)
        {
            recordedLocalPoint = BezierPointWrapper.CurrentValue;
            recordedType = draggingHandleType;
        }

        public void CommitSubPointPos(Vector2 newLocalPoint, BezierPointSubItemType type)
        {
            if (recordedLocalPoint == null || recordedType == null)
                throw new Exception("记录的开始拖拽点位置或类型为空！");
            if (recordedType != type)
                throw new Exception("结束拖拽时的 type 与开始拖拽时记录的不一致，请检查！");

            GetPointDataByLocalPoint(newLocalPoint, type, out BezierPoint newBezierPoint);
            BezierPoint oldBezierPoint = (BezierPoint)recordedLocalPoint;

            CommandStack.ExecuteCommand(
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        BezierPointWrapper,
                        newBezierPoint
                    );
                },
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        BezierPointWrapper,
                        oldBezierPoint
                    );
                }
            );

            recordedLocalPoint = null;
            recordedType = null;
        }

        public void OnDoubleClick()
        {
            if (IsFirstPoint && IsLastPoint)
                return;

            bool canActiveLeftControlPoint =
                !IsFirstPoint && BezierPointWrapper.CurrentValue.LeftControlPoint == BezierPointWrapper.CurrentValue.PositionPoint;
            bool canActiveRightControlPoint =
                !IsLastPoint && BezierPointWrapper.CurrentValue.RightControlPoint == BezierPointWrapper.CurrentValue.PositionPoint;

            BezierPoint oldBezierPoint = BezierPointWrapper.CurrentValue;
            BezierPoint newBezierPoint;

            if (canActiveLeftControlPoint || canActiveRightControlPoint)
            {
                // 如果至少有一个点可以激活，则把那些能激活的点都激活了
                BezierPointPos pos = oldBezierPoint.PositionPoint;
                BezierPointPos leftPos = canActiveLeftControlPoint ? new BezierPointPos(pos.MsTime, pos.Value - 100f) : pos;
                BezierPointPos rightPos = canActiveRightControlPoint ? new BezierPointPos(pos.MsTime, pos.Value + 100f) : pos;
                newBezierPoint = new BezierPoint(pos, leftPos, rightPos);
            }
            else
            {
                // 如果所有点都已经激活了，则取消激活所有的点
                BezierPointPos pos = oldBezierPoint.PositionPoint;
                newBezierPoint = new BezierPoint(pos, pos, pos);
            }

            CommandStack.ExecuteCommand(
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        BezierPointWrapper,
                        newBezierPoint
                    );
                },
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        BezierPointWrapper,
                        oldBezierPoint
                    );
                }
            );
        }


        /// <summary>
        /// 根据当前的局部位置和拖动点种类，计算贝塞尔点三个子控制点的坐标
        /// </summary>
        /// <remarks>最终输出的坐标会确保整条曲线合法不折返，且拖动位置点时会同步更新左右控制点位置</remarks>
        private void GetPointDataByLocalPoint(Vector2 localPoint, BezierPointSubItemType type, out BezierPoint bezierPoint)
        {
            // 将本地坐标转换到贝塞尔点数据位置
            float msTime =
                localPoint.x / SpeedTemplateCurveFrameViewModel.ScaleX.CurrentValue
                - SpeedTemplateCurveFrameViewModel.OffsetX.CurrentValue;
            float value =
                localPoint.y / SpeedTemplateCurveFrameViewModel.ScaleY.CurrentValue
                - SpeedTemplateCurveFrameViewModel.OffsetY.CurrentValue;

            // TODO: 结合整条贝塞尔曲线组，将初步转换的数据进一步转换到合法的数据，避免异常

            switch (type)
            {
                case BezierPointSubItemType.PosPoint:
                    float offsetMsTime = msTime - ((BezierPoint)recordedLocalPoint).PositionPoint.MsTime;
                    float offsetValue = value - ((BezierPoint)recordedLocalPoint).PositionPoint.Value;

                    bezierPoint = new BezierPoint(
                        new BezierPointPos(Mathf.RoundToInt(msTime), value),
                        new BezierPointPos(
                            ((BezierPoint)recordedLocalPoint).LeftControlPoint.MsTime + Mathf.RoundToInt(offsetMsTime),
                            ((BezierPoint)recordedLocalPoint).LeftControlPoint.Value + offsetValue
                        ),
                        new BezierPointPos(
                            ((BezierPoint)recordedLocalPoint).RightControlPoint.MsTime + Mathf.RoundToInt(offsetMsTime),
                            ((BezierPoint)recordedLocalPoint).RightControlPoint.Value + offsetValue
                        )
                    );
                    break;
                case BezierPointSubItemType.LeftControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(BezierPointWrapper.CurrentValue.PositionPoint.MsTime, BezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos((int)msTime, value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.RightControlPoint.MsTime, BezierPointWrapper.CurrentValue.RightControlPoint.Value)
                    );
                    break;
                case BezierPointSubItemType.RightControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(BezierPointWrapper.CurrentValue.PositionPoint.MsTime, BezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos(BezierPointWrapper.CurrentValue.LeftControlPoint.MsTime, BezierPointWrapper.CurrentValue.LeftControlPoint.Value),
                        new BezierPointPos((int)msTime, value)
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
