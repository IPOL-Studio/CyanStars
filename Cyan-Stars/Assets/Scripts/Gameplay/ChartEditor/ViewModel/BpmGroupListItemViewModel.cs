#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupListItemViewModel : BaseViewModel
    {
        private readonly BpmGroupViewModel BpmGroupViewModel;
        private readonly BpmGroupItem BpmItem;

        public BpmGroupListItemViewModel(
            ChartEditorModel model,
            CommandManager commandManager,
            BpmGroupViewModel bpmGroupViewModel,
            BpmGroupItem bpmItem
        )
            : base(model, commandManager)
        {
            BpmGroupViewModel = bpmGroupViewModel;
            BpmItem = bpmItem;
        }
    }
}
