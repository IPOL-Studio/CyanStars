#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupListItemViewModel : BaseViewModel
    {
        private readonly BpmGroupViewModel BpmGroupViewModel;
        private readonly BpmItem BpmItem;

        public BpmGroupListItemViewModel(
            ChartEditorModel model,
            CommandManager commandManager,
            BpmGroupViewModel bpmGroupViewModel,
            BpmItem bpmItem
        )
            : base(model, commandManager)
        {
            BpmGroupViewModel = bpmGroupViewModel;
            BpmItem = bpmItem;
        }
    }
}
