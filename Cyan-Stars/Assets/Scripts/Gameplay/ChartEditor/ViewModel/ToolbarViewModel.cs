#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ToolbarViewModel : BaseViewModel
    {
        public readonly BindableProperty<EditToolType> SelectedEditTool = new BindableProperty<EditToolType>();

        /// <summary>
        /// 构造与绑定
        /// </summary>
        public ToolbarViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            SelectedEditTool.Value = Model.SelectedEditTool.Value;

            Model.SelectedEditTool.OnValueChanged += value =>
            {
                SelectedEditTool.Value = value;
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
