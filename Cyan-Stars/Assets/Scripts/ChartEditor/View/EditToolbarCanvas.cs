// TODO：需要修改，将默认工具初始化放在 VM 层硬编码

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
                throw new System.InvalidOperationException("EditToolbar: 未在 Unity 中正确配置");
            }

            // 传入初始激活的工具
            Toggle initialToggle = null; // 应该在 Unity 编辑器中将一个 toggle 设为启用
            for (int i = 0; i < toggles.Length; i++)
            {
                if (!toggles[i].isOn)
                {
                    continue;
                }

                if (initialToggle is null)
                {
                    initialToggle = toggles[i];
                    ViewModel.ChangeEditTool((EditTools)i);
                }
                else
                {
                    Debug.LogError("EditToolbar: 初始存在多个激活的 toggle，在 Unity 内修改");
                    throw new Exception("EditToolbar: 存在多个激活的 toggle，在 Unity 内修改");
                }
            }

            if (initialToggle is null)
            {
                Debug.LogError("EditToolbar: 未设置初始激活的 toggle，在 Unity 内修改");
                throw new Exception("EditToolbar: 未设置初始激活的 toggle，在 Unity 内修改");
            }


            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] is null)
                {
                    Debug.LogError("EditToolbar: null toggle");
                    throw new InvalidOperationException("EditToolbar: null toggle");
                }

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
