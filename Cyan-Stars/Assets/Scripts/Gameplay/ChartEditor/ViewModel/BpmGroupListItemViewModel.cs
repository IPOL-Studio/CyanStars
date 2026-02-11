#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class BpmGroupListItemViewModel : BaseViewModel
    {
        private readonly BpmGroupViewModel BpmGroupViewModel;
        private readonly BpmGroupItem BpmItem;

        public readonly int ItemIndex;

        public readonly ReadOnlyReactiveProperty<bool> IsSelected;

        private readonly ReactiveProperty<string> beatAndTimeString;
        public ReadOnlyReactiveProperty<string> BeatAndTimeString => beatAndTimeString;


        public BpmGroupListItemViewModel(
            ChartEditorModel model,
            BpmGroupViewModel bpmGroupViewModel,
            BpmGroupItem bpmItem,
            IList<BpmGroupItem> bpmListItems
        )
            : base(model)
        {
            BpmGroupViewModel = bpmGroupViewModel;
            BpmItem = bpmItem;

            int index = bpmListItems.IndexOf(BpmItem);
            ItemIndex = index;

            IsSelected = BpmGroupViewModel.SelectedBpmItem
                .Select(data => data == BpmItem)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            beatAndTimeString = new ReactiveProperty<string>("");

            BpmGroupViewModel.BpmGroupDataChangedSubject
                .Subscribe(changedItemIndex =>
                    {
                        if (changedItemIndex > ItemIndex)
                            return;

                        // [0, 1, 2]
                        string beatPart =
                            $"[{BpmItem.StartBeat.IntegerPart}, {BpmItem.StartBeat.Numerator}, {BpmItem.StartBeat.Denominator}]";

                        // 0:59.900 或 199:59.999
                        int ms = BpmGroupHelper.CalculateTime(bpmListItems, bpmItem.StartBeat);
                        int minutes = ms / 60000;
                        int seconds = (ms / 1000) % 60;
                        int milliseconds = ms % 1000;
                        string timePart = $"{minutes}:{seconds:D2}.{milliseconds:D3}";

                        beatAndTimeString.Value = $"{timePart}\n{beatPart}";
                    }
                )
                .AddTo(base.Disposables);
        }


        public void OnToggleChanged(bool isOn)
        {
            if (!isOn)
                return; // 由 Unity 的 ToggleGroup 自动取消，无需处理

            BpmGroupViewModel.SelectBpmItem(BpmItem);
        }
    }
}
