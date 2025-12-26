#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ToolbarViewModel : BaseViewModel
    {
        public readonly ReactiveProperty<EditToolType> SelectedEditTool;

        /// <summary>
        /// 构造与绑定
        /// </summary>
        public ToolbarViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            SelectedEditTool = Model.SelectedEditTool;
        }
    }
}
