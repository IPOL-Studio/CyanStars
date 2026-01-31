#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ToolbarViewModel : BaseViewModel
    {
        public readonly ReadOnlyReactiveProperty<EditToolType> SelectedEditTool;

        /// <summary>
        /// 构造与绑定
        /// </summary>
        public ToolbarViewModel(ChartEditorModel model)
            : base(model)
        {
            SelectedEditTool = Model.SelectedEditTool.AddTo(Disposables);
        }

        public void SetSelectedTool(EditToolType tool)
        {
            if (Model.SelectedEditTool.CurrentValue != tool)
                Model.SelectedEditTool.Value = tool;
        }
    }
}
