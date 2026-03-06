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


        public void DeletePoint()
        {
            BezierPoint bezierPoint = BezierPointWrapper.CurrentValue;
            CommandStack.ExecuteCommand(
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryRemovePoint(
                        BezierPointWrapper
                    );
                },
                () =>
                {
                    SpeedTemplateCurveFrameViewModel.SelectedSpeedTemplateData.CurrentValue.BezierCurves.TryAddPoint(
                        bezierPoint
                    );
                }
            );
        }

        public void SelectPoint()
        {
            if (SpeedTemplateCurveFrameViewModel.SelectedPoint.CurrentValue == BezierPointWrapper)
                return;

            SpeedTemplateCurveFrameViewModel.SelectPoint(BezierPointWrapper);
        }

        public void UpdateSubPointPos(Vector2 localPoint, BezierPointSubItemType type)
        {
            SpeedTemplateCurveFrameViewModel.UpdateSubPointPos(BezierPointWrapper, localPoint, type);
        }

        public void RecordSubPointPos(BezierPointSubItemType draggingHandleType)
        {
            SpeedTemplateCurveFrameViewModel.RecordSubPointPos(BezierPointWrapper, draggingHandleType);
        }

        public void CommitSubPointPos(Vector2 newLocalPoint, BezierPointSubItemType type)
        {
            SpeedTemplateCurveFrameViewModel.CommitSubPointPos(BezierPointWrapper, newLocalPoint, type);
        }

        public void OnDoubleClick()
        {
            SpeedTemplateCurveFrameViewModel.OnDoubleClick(BezierPointWrapper);
        }
    }
}
