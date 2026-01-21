#nullable enable

using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class NoteAttributeViewModel : BaseViewModel
    {
        public NoteAttributeViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
        }
    }
}
