#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ToolbarView : BaseView<ToolbarViewModel>
    {
        [SerializeField]
        private List<ToolbarData> tools = null!;


        public override void Bind(ToolbarViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.SelectedEditTool
                .Subscribe(selectedTool =>
                    {
                        tools.Find(t => t.ToolType == selectedTool).ToolToggle.isOn = true;
                    }
                )
                .AddTo(this);

            foreach (var tool in tools)
            {
                tool.ToolToggle.onValueChanged
                    .AddListener(isOn =>
                        {
                            if (!isOn)
                                return; // Unity ToggleGroup 自动取消选中，无需修改属性

                            ViewModel.SetSelectedTool(tool.ToolType);
                        }
                    );
            }
        }

        [Serializable]
        private class ToolbarData
        {
            public EditToolType ToolType;
            public Toggle ToolToggle = null!;
        }


        protected override void OnDestroy()
        {
            foreach (var tool in tools)
            {
                tool.ToolToggle.onValueChanged.RemoveAllListeners();
            }
        }
    }
}
