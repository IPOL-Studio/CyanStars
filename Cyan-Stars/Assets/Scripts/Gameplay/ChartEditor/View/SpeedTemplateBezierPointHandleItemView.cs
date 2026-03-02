#nullable enable

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
                        // 当 BezierPoint 结构体中任意字段变化时，更新 SubPoints 和 uiLineRenderer 屏幕坐标
                        ((RectTransform)transform).anchoredPosition =
                            new Vector2(point.PositionPoint.MsTime, point.PositionPoint.Value);
                        ((RectTransform)leftControlPointObject.transform).anchoredPosition =
                            new Vector2(point.LeftControlPoint.MsTime - point.PositionPoint.MsTime, point.LeftControlPoint.Value - point.PositionPoint.Value);
                        ((RectTransform)rightControlPointObject.transform).anchoredPosition =
                            new Vector2(point.RightControlPoint.MsTime - point.PositionPoint.MsTime, point.RightControlPoint.Value - point.PositionPoint.Value);

                        uiLineRenderer.Points = new[] { ((RectTransform)leftControlPointObject.transform).anchoredPosition - ((RectTransform)posPointObject.transform).anchoredPosition, Vector2.zero, ((RectTransform)rightControlPointObject.transform).anchoredPosition - ((RectTransform)posPointObject.transform).anchoredPosition };
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            Debug.Log(localPoint);
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
