using System;
using CyanStars.ChartEditor.Model;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    [RequireComponent(typeof(ToggleGroup))]
    public class Toolbar : BaseView
    {
        /// <summary>
        /// Toggle 列表，注意：目前为硬编码，要求 Unity 编辑器中按 EditTool 枚举顺序正确配置 7 个 Toggle
        /// </summary>
        [SerializeField]
        private Toggle[] toggles;

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            if (!CheckToggles())
            {
                Debug.LogError("Toolbar: Toggles 检查失败，请在编辑器内正确配置");
                throw new Exception();
            }

            for (int i = 0; i < toggles.Length; i++)
            {
                // 初始化默认值
                toggles[i].isOn = ((int)Model.SelectedEditTool == i);

                // 添加事件绑定
                int index = i;
                toggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn) // 由 Unity ToggleGroup 自动取消的 Toggle 不会被处理
                    {
                        Model.SetEditTool((EditTool)index);
                    }
                });
            }

            Model.OnEditToolChanged += OnEditToolChanged;
        }

        /// <summary>
        /// 初步检查是否为 Toggles 赋值
        /// </summary>
        /// <remarks>注意：尚无法检查是否正确绑定物体</remarks>
        private bool CheckToggles()
        {
            if (toggles == null || toggles.Length != 7)
            {
                return false;
            }

            foreach (Toggle toggle in toggles)
            {
                if (toggle == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnEditToolChanged()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i].isOn != ((int)Model.SelectedEditTool == i))
                {
                    toggles[i].isOn = ((int)Model.SelectedEditTool == i);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var toggle in toggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }

            if (Model != null)
            {
                Model.OnEditToolChanged -= OnEditToolChanged;
            }
        }
    }
}
