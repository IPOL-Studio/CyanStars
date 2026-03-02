#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class SpeedTemplateBezierPointHandleItemView : BaseView<SpeedTemplateBezierPointHandleItemViewModel>
    {
        [SerializeField]
        private UILineRenderer uiLineRenderer = null!;

        [SerializeField]
        private GameObject pointFrameObject = null!;


        [SerializeField]
        private GameObject posPointObject = null!;

        [SerializeField]
        private GameObject leftControlPointObject = null!;

        [SerializeField]
        private GameObject rightControlPointObject = null!;


        /// <summary>
        /// 绑定方法，在实例化 go 后立刻调用
        /// </summary>
        public override void Bind(SpeedTemplateBezierPointHandleItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            uiLineRenderer.Points = new[] { ViewModel.BezierPointWrapper.CurrentValue.LeftControlPoint.ToVector2() - ViewModel.BezierPointWrapper.CurrentValue.PositionPoint.ToVector2(), Vector2.zero, ViewModel.BezierPointWrapper.CurrentValue.RightControlPoint.ToVector2() - ViewModel.BezierPointWrapper.CurrentValue.PositionPoint.ToVector2() };

            ViewModel.SelfSelected
                .Subscribe(isSelected =>
                    {
                        uiLineRenderer.enabled = isSelected;
                        leftControlPointObject.SetActive(isSelected);
                        rightControlPointObject.SetActive(isSelected);
                    }
                )
                .AddTo(this);

            ViewModel.BezierPointWrapper
                .Subscribe(point =>
                    {
                        ((RectTransform)transform).anchoredPosition =
                            new Vector2(point.PositionPoint.MsTime, point.PositionPoint.Value);
                        ((RectTransform)leftControlPointObject.transform).anchoredPosition =
                            new Vector2(point.LeftControlPoint.MsTime - point.PositionPoint.MsTime, point.LeftControlPoint.Value - point.PositionPoint.Value);
                        ((RectTransform)rightControlPointObject.transform).anchoredPosition =
                            new Vector2(point.RightControlPoint.MsTime - point.PositionPoint.MsTime, point.RightControlPoint.Value - point.PositionPoint.Value);
                    }
                )
                .AddTo(this);
        }


        #region 位置点、控制点被点击和拖拽回调

        public void OnSubObjectPointClick(PointerEventData eventData, BezierPointSubItemType type)
        {
            if (type != BezierPointSubItemType.PosPoint)
                return;

            ViewModel.SelectPoint();
        }

        public void OnSubObjectDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
            switch (type)
            {
                case BezierPointSubItemType.PosPoint:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        (RectTransform)transform.parent,
                        eventData.position,
                        eventData.pressEventCamera,
                        out Vector2 localPoint
                    );
                    ((RectTransform)transform).anchoredPosition = localPoint;
                    break;
                case BezierPointSubItemType.LeftControlPoint:
                    break;
                case BezierPointSubItemType.RightControlPoint:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void OnSubObjectBeginDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
        }

        public void OnSubObjectEndDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
        }

        #endregion
    }
}
