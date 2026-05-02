#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 动态生成。关闭精简模式后 BpmGroup 弹窗左侧的 ListItem
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class BpmGroupListItemView : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text itemNumberText = null!;

        [SerializeField]
        private TMP_Text beatAndTimeText = null!;


        private Toggle itemToggle = null!;

        private CompositeDisposable? disposables;


        public void Awake()
        {
            itemToggle = GetComponent<Toggle>();
        }

        public void Bind(BpmGroupViewModel targetViewModel, ToggleGroup bpmListToggleGroup, int index)
        {
            destroyCancellationToken.ThrowIfCancellationRequested();

            TryReleaseBind();
            disposables = new CompositeDisposable();

            itemToggle.group = bpmListToggleGroup;

            itemNumberText.text = $"#{index + 1}";
            beatAndTimeText.text = GetDetailText(targetViewModel, index);

            targetViewModel.SelectedBpmItemIndex
                .Subscribe(i => itemToggle.isOn = i == index)
                .AddTo(disposables);
            targetViewModel.BpmGroupDataChangedSubject
                .Subscribe(changedItemIndex =>
                {
                    if (changedItemIndex > index)
                        return;

                    beatAndTimeText.text = GetDetailText(targetViewModel, index);
                })
                .AddTo(disposables);

            itemToggle
                .OnValueChangedAsObservable()
                .Subscribe((targetViewModel, index), static (isOn, state) =>
                {
                    if (isOn)
                        state.targetViewModel.SelectBpmItem(state.targetViewModel.BpmItems[state.index]);
                })
                .AddTo(disposables);
        }

        private void OnDestroy()
        {
            TryReleaseBind();
        }

        public void TryReleaseBind()
        {
            disposables?.Dispose();
            disposables = null;
        }

        private string GetDetailText(BpmGroupViewModel targetViewModel, int index)
        {
            var bpmItem = targetViewModel.BpmItems[index];

            // [0, 1, 2]
            string beatPart =
                $"[{bpmItem.StartBeat.IntegerPart}, {bpmItem.StartBeat.Numerator}, {bpmItem.StartBeat.Denominator}]";

            // 0:59.900 或 199:59.999
            int ms = BpmGroupHelper.CalculateTime(targetViewModel.BpmItems, bpmItem.StartBeat);
            int minutes = ms / 60000;
            int seconds = (ms / 1000) % 60;
            int milliseconds = ms % 1000;
            string timePart = $"{minutes}:{seconds:D2}.{milliseconds:D3}";

            return $"{beatPart}\n{timePart}";
        }
    }
}
