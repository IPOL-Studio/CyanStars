using System;
using UnityEngine;
using UnityEngine.UI;
using CyanStars.ChartEditor.ViewModel;

namespace CyanStars.ChartEditor.View
{
    public enum EditTools
    {
        Select,
        TapPen,
        DragPen,
        HoldPen,
        ClickPen,
        BreakPen,
        Eraser
    }

    /// <summary>
    /// 左侧编辑栏的 view 层
    /// </summary>
    /// <remarks>工具顺序暂时是硬编码的，务必确保编辑器内正确设置</remarks>
    [RequireComponent(typeof(ToggleGroup))]
    public class EditToolbarCanvas : BaseView
    {
        [SerializeField]
        private Toggle[] toggles;


        public override void Bind(MainViewModel mainViewModel)
        {
            base.Bind(mainViewModel);

            if (toggles.Length != 7) // 目前是硬编码的，需要确保在 Unity 中按枚举的顺序正确配置 toggle
            {
                Debug.LogError("EditToolbar: 未在 Unity 中正确配置");
                throw new InvalidOperationException("EditToolbar: 未在 Unity 中正确配置");
            }

            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] is null)
                {
                    Debug.LogError("EditToolbar: null toggle");
                    throw new InvalidOperationException("EditToolbar: null toggle");
                }

                // 如果 VM 中将这个 toggle 设为了默认初始启用，则修改 isOn 为 true，否则为 false
                toggles[i].isOn = (i == (int)ViewModel.SelectedEditTool);

                // 绑定事件响应，使新选中的物体调用 VM 的方法，传入新的 EditTools 类型
                int toggleIndex = i;
                toggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) //被 Unity 自动取消选中的物体不会调用方法
                    {
                        ViewModel.ChangeEditTool((EditTools)toggleIndex);
                    }
                });
            }
        }


        private void OnDestroy()
        {
            foreach (Toggle toggle in toggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }
}
