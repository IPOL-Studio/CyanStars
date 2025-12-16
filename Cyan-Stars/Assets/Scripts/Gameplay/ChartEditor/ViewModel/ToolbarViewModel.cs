#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ToolbarViewModel : BaseViewModel
    {
        /// <summary>
        /// 选中的 EditTool 变化时通知 View 刷新
        /// </summary>
        public event Action<EditToolType>? OnEditToolChanged;

        /// <summary>
        /// 构造与绑定
        /// </summary>
        public ToolbarViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            Model.SelectedEditTool.OnValueChanged += (tool) =>
            {
                OnEditToolChanged?.Invoke(tool);
            };
        }

        /// <summary>
        /// View 传入绑定
        /// </summary>
        public void SelectTool(EditToolType editToolType)
        {
            if (editToolType == Model.SelectedEditTool.Value)
            {
                return;
            }

            // 修改工具的行为目前不需要支持撤销重做，直接修改即可
            Model.SelectedEditTool.Value = editToolType;
        }
    }
}
