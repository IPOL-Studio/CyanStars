#nullable enable

using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace CyanStars.Gameplay.ChartEditor
{
    public class SpeedTemplateBezierPointHandleItemView : BaseView<SpeedTemplateBezierPointHandleItemViewModel>
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
    }
}
