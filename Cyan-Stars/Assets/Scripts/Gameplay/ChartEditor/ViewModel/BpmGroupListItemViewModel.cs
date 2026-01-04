#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupListItemViewModel : BaseViewModel
    {
        private readonly BpmGroupViewModel BpmGroupViewModel;
        private readonly BpmGroupItem BpmGroupItem;

        public BpmGroupListItemViewModel(
            ChartEditorModel model,
            CommandManager commandManager,
            BpmGroupViewModel bpmGroupViewModel,
            BpmGroupItem bpmGroupItem
        )
            : base(model, commandManager)
        {
            BpmGroupViewModel = bpmGroupViewModel;
            BpmGroupItem = bpmGroupItem;
        }
    }
}
