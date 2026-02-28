#nullable enable

using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace CyanStars.Gameplay.ChartEditor
{
    public class SpeedTemplateBezierPointHandleItemView : BaseView<SpeedTemplateBezierPointHandleItemViewModel>, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private UILineRenderer uiLineRenderer = null!;


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
                        uiLineRenderer.enabled = false;
                        leftControlPointObject.SetActive(isSelected);
                        rightControlPointObject.SetActive(isSelected);
                    }
                )
                .AddTo(this);
        }


        #region 位置点、控制点被点击和拖拽回调

        // 由 pointObject 的 EventTrigger 组件触发，请在 Unity 内检查脚本挂载

        public void OnPosPointClicked()
        {
        }

        public void OnPosPointDragging()
        {
        }

        public void OnLeftControlPointDragging()
        {
        }

        public void OnRightControlPointDragging()
        {
        }

        #endregion

        public void OnPointerDown(PointerEventData eventData)
        {
            // 点击时将当前 Handle 置于最上层，防止被其他 Handle 遮挡导致无法拖拽
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // TODO: 请求 VM 更新预览位置
        }

        public void OnBeginDrag(PointerEventData _)
        {
            // TODO: 请求 VM 记录初始位置以便撤销
        }

        public void OnEndDrag(PointerEventData _)
        {
            // TODO: 向 VM 提交位置更新
        }
    }
}
