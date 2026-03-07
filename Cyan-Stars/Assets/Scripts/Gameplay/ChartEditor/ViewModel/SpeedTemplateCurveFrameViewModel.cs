#nullable enable

using System;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateCurveFrameViewModel : BaseViewModel
    {
        private readonly SpeedTemplateViewModel SpeedTemplateViewModel;

        // 各个坐标点映射到 CurveFrame 时所用的偏移和缩放。
        // 先将数据坐标与 Offset 相加，然后得到的结果乘以 Scale，即可得到相对于 CurveFrame 局部坐标。
        // TODO: 改为动态拓展的视窗范围
        private const float DefaultViewportX = 1000f;
        private const float DefaultViewportY = 1000f;
        private readonly ReactiveProperty<float> scaleX = new ReactiveProperty<float>(1f);
        private readonly ReactiveProperty<float> scaleY = new ReactiveProperty<float>(1f);
        private readonly ReactiveProperty<float> offsetX = new ReactiveProperty<float>(0f);
        private readonly ReactiveProperty<float> offsetY = new ReactiveProperty<float>(0f);
        public ReadOnlyReactiveProperty<float> ScaleX => scaleX;
        public ReadOnlyReactiveProperty<float> ScaleY => scaleY;
        public ReadOnlyReactiveProperty<float> OffsetX => offsetX;
        public ReadOnlyReactiveProperty<float> OffsetY => offsetY;

        // 用于显示拖拽/添加贝塞尔点时的边界
        public int MinMsTime { get; private set; } = 0;
        public int MaxMsTime { get; private set; } = int.MaxValue;


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


        /// <summary>
        /// 当前选中的变速模板的贝塞尔点 VM 映射表
        /// </summary>
        private readonly ReactiveProperty<ISynchronizedView<ReactiveProperty<BezierPoint>, SpeedTemplateBezierPointHandleItemViewModel>?> bezierPointViewModelsMap = new();

        public ReadOnlyReactiveProperty<ISynchronizedView<ReactiveProperty<BezierPoint>, SpeedTemplateBezierPointHandleItemViewModel>?> BezierPointViewModelsMap => bezierPointViewModelsMap;


        // 开始拖拽时记录的贝塞尔点位置和类型，位置用于撤销命令和拖拽位置点时计算控制点相对位移
        private readonly ReactiveProperty<BezierPoint?> recordedLocalPoint = new ReactiveProperty<BezierPoint?>();
        public ReadOnlyReactiveProperty<BezierPoint?> RecordedLocalPoint => recordedLocalPoint;
        private BezierPointSubItemType? recordedType = null;


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

            // 切换选中变速模板时重新构建子 VM 和 V
            SelectedSpeedTemplateData
                .Subscribe(selectedTemplateData =>
                    {
                        bezierPointViewModelsMap.CurrentValue?.Dispose();

                        if (selectedTemplateData != null)
                        {
                            bezierPointViewModelsMap.Value = selectedTemplateData.BezierCurves.Points
                                .CreateView(pointWrapper =>
                                    new SpeedTemplateBezierPointHandleItemViewModel(
                                        Model,
                                        this,
                                        pointWrapper
                                    )
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


        public void OnHorizontalChanged(float lowX, float highX)
        {
            if (SelectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("选中曲线为空时不应该调整缩放");

            scaleX.Value = 1 / ((highX - lowX) / DefaultViewportX);
            offsetX.Value = -lowX;
        }

        public void OnVerticalChanged(float lowY, float highY)
        {
            if (SelectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("选中曲线为空时不应该调整缩放");

            scaleY.Value = 1 / ((highY - lowY) / DefaultViewportY);
            offsetY.Value = -((highY + lowY) / 2f);
        }


        public void SelectPoint(ReadOnlyReactiveProperty<BezierPoint>? bezierPointWrapper)
        {
            selectedPoint.Value = bezierPointWrapper;
        }

        public bool TryAddPoint(Vector2 position)
        {
            BezierPointPos bezierPointPos = new(
                Mathf.RoundToInt(position.x / ScaleX.CurrentValue - OffsetX.CurrentValue),
                position.y / ScaleY.CurrentValue - OffsetY.CurrentValue
            );

            return SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryAddPoint(
                new BezierPoint(bezierPointPos, bezierPointPos, bezierPointPos)
            );
        }

        public void UpdateSubPointPos(ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper, Vector2 localPoint, BezierPointSubItemType type)
        {
            if (recordedLocalPoint == null || recordedType == null)
                throw new Exception("记录的开始拖拽点位置或类型为空！");
            if (recordedType != type)
                throw new Exception("拖拽时的 type 与开始拖拽时记录的不一致，请检查！");

            GetPointDataByLocalPoint(bezierPointWrapper, localPoint, type, out BezierPoint bezierPoint);
            SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                bezierPointWrapper,
                bezierPoint
            );
        }

        public void RecordSubPointPos(ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper, BezierPointSubItemType draggingHandleType)
        {
            recordedLocalPoint.Value = bezierPointWrapper.CurrentValue;
            recordedType = draggingHandleType;
        }

        public void CommitSubPointPos(ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper, Vector2 newLocalPoint, BezierPointSubItemType type)
        {
            if (recordedLocalPoint == null || recordedType == null)
                throw new Exception("记录的开始拖拽点位置或类型为空！");
            if (recordedType != type)
                throw new Exception("结束拖拽时的 type 与开始拖拽时记录的不一致，请检查！");

            GetPointDataByLocalPoint(bezierPointWrapper, newLocalPoint, type, out BezierPoint newBezierPoint);
            BezierPoint oldBezierPoint = (BezierPoint)RecordedLocalPoint.CurrentValue;

            CommandStack.ExecuteCommand(
                () =>
                {
                    SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        bezierPointWrapper,
                        newBezierPoint
                    );
                },
                () =>
                {
                    SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        bezierPointWrapper,
                        oldBezierPoint
                    );
                }
            );

            recordedLocalPoint.Value = null;
            recordedType = null;
        }

        /// <summary>
        /// 当贝塞尔点被双击时，显示或隐藏其控制点
        /// </summary>
        /// <param name="bezierPointWrapper">被双击的贝塞尔点</param>
        public void OnPointDoubleClick(ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper)
        {
            bool isFirstPoint =
                SpeedTemplateViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.Points[0] == bezierPointWrapper;
            bool isLastPoint =
                SpeedTemplateViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.Points[^1] == bezierPointWrapper;

            // 如果整条曲线就这一个点，双击将不起作用
            if (isFirstPoint && isLastPoint)
                return;

            bool canActiveLeftControlPoint =
                !isFirstPoint && bezierPointWrapper.CurrentValue.LeftControlPoint == bezierPointWrapper.CurrentValue.PositionPoint;
            bool canActiveRightControlPoint =
                !isLastPoint && bezierPointWrapper.CurrentValue.RightControlPoint == bezierPointWrapper.CurrentValue.PositionPoint;

            BezierPoint oldBezierPoint = bezierPointWrapper.CurrentValue;
            BezierPoint newBezierPoint;

            if (canActiveLeftControlPoint || canActiveRightControlPoint)
            {
                // 如果至少有一个点可以激活，则把那些能激活的点都激活了
                BezierPointPos pos = oldBezierPoint.PositionPoint;
                BezierPointPos leftPos = canActiveLeftControlPoint ? new BezierPointPos(pos.MsTime, pos.Value + 100f) : pos;
                BezierPointPos rightPos = canActiveRightControlPoint ? new BezierPointPos(pos.MsTime, pos.Value - 100f) : pos;
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
                    SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        bezierPointWrapper,
                        newBezierPoint
                    );
                },
                () =>
                {
                    SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryUpdatePoint(
                        bezierPointWrapper,
                        oldBezierPoint
                    );
                }
            );
        }

        /// <summary>
        /// 根据当前的局部位置和拖动点种类，计算贝塞尔点三个子控制点的坐标，并更新 MinMsTime 和 MaxMsTime
        /// </summary>
        /// <remarks>最终输出的坐标会确保曲线符合约束条件，且拖动位置点时会同步更新左右控制点位置</remarks>
        private void GetPointDataByLocalPoint(ReadOnlyReactiveProperty<BezierPoint> bezierPointWrapper, Vector2 localPoint, BezierPointSubItemType type, out BezierPoint bezierPoint)
        {
            // 将指针的本地坐标转换到贝塞尔点数据位置
            float pointX = localPoint.x / ScaleX.CurrentValue - OffsetX.CurrentValue;
            float pointY = localPoint.y / ScaleY.CurrentValue - OffsetY.CurrentValue;

            IReadOnlyObservableList<ReactiveProperty<BezierPoint>> points =
                SelectedSpeedTemplateData.CurrentValue.BezierCurves.Points;
            int pointWrapperIndex =
                SelectedSpeedTemplateData.CurrentValue.BezierCurves.GetPointIndex(bezierPointWrapper);

            //  根据约束条件限制 x 坐标
            int minX, maxX;
            switch (type)
            {
                case BezierPointSubItemType.PosPoint:
                    if (pointWrapperIndex == 0)
                    {
                        minX = 0;
                        maxX = 0;
                        break;
                    }

                    minX = Math.Max(0, bezierPointWrapper.CurrentValue.PositionPoint.MsTime - bezierPointWrapper.CurrentValue.LeftControlPoint.MsTime); // 拖动位置点时，一并移动的左控制点也要大于等于 0
                    maxX = int.MaxValue;

                    // this-1.右控制点 <= this.位置点 && this-1.位置点 <= this.左控制点 && this-1.位置点+1 <= this.位置点
                    if (0 < pointWrapperIndex)
                    {
                        minX = Math.Max(minX, points[pointWrapperIndex - 1].CurrentValue.RightControlPoint.MsTime);
                        minX = Math.Max(minX, points[pointWrapperIndex - 1].CurrentValue.PositionPoint.MsTime +
                                              (bezierPointWrapper.CurrentValue.PositionPoint.MsTime -
                                               bezierPointWrapper.CurrentValue.LeftControlPoint.MsTime));
                        minX = Math.Max(minX, points[pointWrapperIndex - 1].CurrentValue.PositionPoint.MsTime + 1);
                    }

                    // this.位置点 <= this+1.左控制点 && this.右控制点 <= this+1.位置点 && this.位置点 <= this-1.位置点-1
                    if (pointWrapperIndex < points.Count - 1)
                    {
                        maxX = Math.Min(maxX, points[pointWrapperIndex + 1].CurrentValue.LeftControlPoint.MsTime);
                        maxX = Math.Min(maxX, points[pointWrapperIndex + 1].CurrentValue.PositionPoint.MsTime -
                                              (bezierPointWrapper.CurrentValue.RightControlPoint.MsTime -
                                               bezierPointWrapper.CurrentValue.PositionPoint.MsTime));
                        maxX = Math.Min(maxX, points[pointWrapperIndex + 1].CurrentValue.PositionPoint.MsTime - 1);
                    }

                    break;
                case BezierPointSubItemType.LeftControlPoint:
                    // 确保左控制点 x 小于等于自身位置点
                    minX = 0;
                    maxX = bezierPointWrapper.CurrentValue.PositionPoint.MsTime;

                    if (0 < pointWrapperIndex)
                    {
                        minX = Math.Max(minX, points[pointWrapperIndex - 1].CurrentValue.PositionPoint.MsTime);
                    }

                    break;
                case BezierPointSubItemType.RightControlPoint:
                    // 确保左控制点 x 大于等于自身位置点
                    minX = bezierPointWrapper.CurrentValue.PositionPoint.MsTime;
                    maxX = int.MaxValue;

                    if (pointWrapperIndex < points.Count - 1)
                    {
                        maxX = Math.Min(maxX, points[pointWrapperIndex + 1].CurrentValue.PositionPoint.MsTime);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            // 将最后用于赋值的 finalX 限制在约束范围内
            int finalX = Mathf.RoundToInt(Mathf.Clamp(pointX, minX, maxX));

            // 赋值贝塞尔点
            switch (type)
            {
                case BezierPointSubItemType.PosPoint:
                    // 如果拖动的是位置点，还要一并更新左右控制点的坐标
                    // 获取开始拖拽时的贝塞尔点数据位置，计算偏移量
                    BezierPoint recordedPoint = (BezierPoint)RecordedLocalPoint.CurrentValue;
                    int recordX = recordedPoint.PositionPoint.MsTime;
                    float recordY = recordedPoint.PositionPoint.Value;

                    int offsetMsTime = finalX - recordX;
                    float offsetValue = pointY - recordY;

                    bezierPoint = new BezierPoint(
                        new BezierPointPos(finalX, pointY),
                        new BezierPointPos(
                            recordedPoint.LeftControlPoint.MsTime + offsetMsTime,
                            recordedPoint.LeftControlPoint.Value + offsetValue
                        ),
                        new BezierPointPos(
                            recordedPoint.RightControlPoint.MsTime + offsetMsTime,
                            recordedPoint.RightControlPoint.Value + offsetValue
                        )
                    );
                    break;
                case BezierPointSubItemType.LeftControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(bezierPointWrapper.CurrentValue.PositionPoint.MsTime, bezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos(finalX, pointY),
                        new BezierPointPos(bezierPointWrapper.CurrentValue.RightControlPoint.MsTime, bezierPointWrapper.CurrentValue.RightControlPoint.Value)
                    );
                    break;
                case BezierPointSubItemType.RightControlPoint:
                    bezierPoint = new BezierPoint(
                        new BezierPointPos(bezierPointWrapper.CurrentValue.PositionPoint.MsTime, bezierPointWrapper.CurrentValue.PositionPoint.Value),
                        new BezierPointPos(bezierPointWrapper.CurrentValue.LeftControlPoint.MsTime, bezierPointWrapper.CurrentValue.LeftControlPoint.Value),
                        new BezierPointPos(finalX, pointY)
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            MinMsTime = minX;
            MaxMsTime = maxX;
        }
    }
}
