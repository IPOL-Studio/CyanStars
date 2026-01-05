#nullable enable

using System.Globalization;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupViewModel : BaseViewModel
    {
        public readonly ISynchronizedView<BpmItem, BpmGroupListItemViewModel> BpmItems;

        private readonly ReactiveProperty<BpmItem?> selectedBpmItem;
        public ReadOnlyReactiveProperty<BpmItem?> SelectedBpmItem => selectedBpmItem;


        public readonly ReadOnlyReactiveProperty<bool> ListVisible;
        public readonly ReadOnlyReactiveProperty<bool> CanvasVisible;

        public readonly ReadOnlyReactiveProperty<string> BPMText;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText1;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText2;
        public readonly ReadOnlyReactiveProperty<string> StartBeatText3;


        public BpmGroupViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            BpmItems = Model.ChartPackData.CurrentValue.BpmGroup
                .CreateView(bpmItem => new BpmGroupListItemViewModel(Model, CommandManager, this, bpmItem))
                .AddTo(base.Disposables);

            selectedBpmItem = new ReactiveProperty<BpmItem?>(null);
            ListVisible = Observable.CombineLatest(
                    Model.IsSimplificationMode,
                    Model.ChartPackData,
                    (isSimple, chartPackData) => !isSimple || (chartPackData?.BpmGroup.Count ?? 0) > 1
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);

            Model.IsSimplificationMode.ToReadOnlyReactiveProperty().AddTo(Disposables);
            CanvasVisible = Model.BpmGroupCanvasVisibility.ToReadOnlyReactiveProperty().AddTo(Disposables);

            BPMText = SelectedBpmItem
                .Select(item => item?.Bpm.ToString(CultureInfo.InvariantCulture) ?? "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText1 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.IntegerPart.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText2 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.Numerator.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
            StartBeatText3 = SelectedBpmItem
                .Select(item => item != null ? item.StartBeat.Denominator.ToString() : "")
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, "")
                .AddTo(Disposables);
        }
    }
}
