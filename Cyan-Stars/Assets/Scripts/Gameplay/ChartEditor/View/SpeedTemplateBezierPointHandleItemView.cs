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
        /// 在一个位置点上两次点击时，小于此间隔视为一次双击
        /// </summary>
        private const float DoubleClickDelaySecond = 0.5f;

        /// <summary>
        /// 上次点击时的 Time.unscaledTime
        /// </summary>
        /// <remarks>如果成功触发双击，将此值设为 0，避免三次点击触发两次双击判断的神秘逻辑</remarks>
        private float lastClickTime = 0;


        /// <summary>
        /// 绑定方法，在实例化 go 后立刻调用
        /// </summary>
        public override void Bind(SpeedTemplateBezierPointHandleItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            Observable.CombineLatest(
                    ViewModel.BezierPointWrapper,
                    ViewModel.OffsetX,
                    ViewModel.ScaleX,
                    ViewModel.OffsetY,
                    ViewModel.ScaleY,
                    ViewModel.SelfSelected,
                    (pointWrapper, offsetX, scaleX, offsetY, scaleY, selected) => (pointWrapper, offsetX, scaleX, offsetY, scaleY, selected)
                )
                .Subscribe(datas =>
                    {
                        leftControlPointObject.SetActive(
                            datas.selected &&
                            datas.pointWrapper.LeftControlPoint != datas.pointWrapper.PositionPoint
                        );
                        rightControlPointObject.SetActive(
                            datas.selected &&
                            datas.pointWrapper.RightControlPoint != datas.pointWrapper.PositionPoint
                        );
                        uiLineRenderer.enabled = datas.selected &&
                                                 (leftControlPointObject.activeSelf || rightControlPointObject.activeSelf);

                        float posX =
                            (datas.pointWrapper.PositionPoint.MsTime + datas.offsetX) * datas.scaleX;
                        float posY =
                            (datas.pointWrapper.PositionPoint.Value + datas.offsetY) * datas.scaleY;
                        ((RectTransform)transform).anchoredPosition =
                            new Vector2(posX, posY);

                        ((RectTransform)posPointObject.transform).anchoredPosition = Vector2.zero;

                        float leftControlSubPointPosX =
                            (datas.pointWrapper.LeftControlPoint.MsTime - datas.pointWrapper.PositionPoint.MsTime) * datas.scaleX;
                        float leftControlSubPointPosY =
                            (datas.pointWrapper.LeftControlPoint.Value - datas.pointWrapper.PositionPoint.Value) * datas.scaleY;
                        ((RectTransform)leftControlPointObject.transform).anchoredPosition =
                            new Vector2(leftControlSubPointPosX, leftControlSubPointPosY);

                        float rightControlSubPointPosX =
                            (datas.pointWrapper.RightControlPoint.MsTime - datas.pointWrapper.PositionPoint.MsTime) * datas.scaleX;
                        float rightControlSubPointPosY =
                            (datas.pointWrapper.RightControlPoint.Value - datas.pointWrapper.PositionPoint.Value) * datas.scaleY;
                        ((RectTransform)rightControlPointObject.transform).anchoredPosition =
                            new Vector2(rightControlSubPointPosX, rightControlSubPointPosY);

                        uiLineRenderer.Points = new[] { new Vector2(leftControlSubPointPosX, leftControlSubPointPosY), new Vector2(0, 0), new Vector2(rightControlSubPointPosX, rightControlSubPointPosY) };
                    }
                )
                .AddTo(this);
        }


        #region 位置点、控制点被点击和拖拽回调

        public void OnSubObjectPointClick(PointerEventData _, BezierPointSubItemType type)
        {
            if (type != BezierPointSubItemType.PosPoint)
                return;

            // 点击时将当前 pointHandle 置于最上层，防止被其他 pointHandle 遮挡导致无法拖拽
            transform.SetAsLastSibling();

            ViewModel.SelectPoint();

            // 双击交互逻辑
            if (Time.unscaledTime - lastClickTime <= DoubleClickDelaySecond)
            {
                // 触发双击
                lastClickTime = 0;
                ViewModel.OnDoubleClick();
            }
            else
            {
                lastClickTime = Time.unscaledTime;
            }
        }

        public void OnSubObjectDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            ViewModel.SetSubPointPos(localPoint, type);
        }

        public void OnSubObjectBeginDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
            // 点击时将当前 pointHandle 置于最上层，防止被其他 pointHandle 遮挡导致无法拖拽
            transform.SetAsLastSibling();

            if (type == BezierPointSubItemType.PosPoint)
                ViewModel.SelectPoint();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            ViewModel.RecordSubPointPos(type);
        }

        public void OnSubObjectEndDrag(PointerEventData eventData, BezierPointSubItemType type)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)transform.parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            ViewModel.CommitSubPointPos(localPoint, type);
        }

        #endregion
    }
}
