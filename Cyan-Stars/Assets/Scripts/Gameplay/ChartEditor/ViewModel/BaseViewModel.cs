using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public abstract class BaseViewModel
    {
        private ChartEditorModel model;
        private CommandManager commandManager;


        protected BaseViewModel(ChartEditorModel model, CommandManager commandManager)
        {
            this.model = model;
            this.commandManager = commandManager;
        }
    }
}
