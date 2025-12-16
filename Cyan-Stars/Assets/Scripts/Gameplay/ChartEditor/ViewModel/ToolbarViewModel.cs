#nullable enable

using System;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ToolbarViewModel : BaseViewModel
    {
        /// <summary>
        /// 选中的 EditTool 变化时通知 View 刷新
        /// </summary>
        public event Action<EditTool>? OnEditToolChanged;

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
        public void SelectTool(EditTool tool)
        {
            if (tool == Model.SelectedEditTool.Value)
            {
                return;
            }

            // 修改工具的行为目前不需要支持撤销重做，直接修改即可
            // CommandManager.ExecuteCommand(new SelectEditTool(Model, Model.SelectedEditTool.Value, tool));
            Model.SelectedEditTool.Value = tool;
        }
    }

    // /// <summary>
    // /// 命令封装
    // /// </summary>
    // public class SelectEditTool : ICommand
    // {
    //     private readonly ChartEditorModel Model;
    //     private readonly EditTool OldEditTool;
    //     private readonly EditTool NewEditTool;
    //
    //     public SelectEditTool(ChartEditorModel model, EditTool oldEditTool, EditTool newEditTool)
    //     {
    //         Model = model;
    //         OldEditTool = oldEditTool;
    //         NewEditTool = newEditTool;
    //     }
    //
    //     public void Execute()
    //     {
    //         Model.SelectedEditTool.Value = NewEditTool;
    //     }
    //
    //     public void Undo()
    //     {
    //         Model.SelectedEditTool.Value = OldEditTool;
    //     }
    // }
}
