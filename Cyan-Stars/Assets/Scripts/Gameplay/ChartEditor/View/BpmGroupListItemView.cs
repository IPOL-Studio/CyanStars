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
    public class BpmGroupListItemView : MonoBehaviour
    {
        [SerializeField]
        private Toggle itemToggle = null!;

        [SerializeField]
        private RawImage ledRawImage = null!;

        [SerializeField]
        private TMP_Text itemNumberText = null!;

        [SerializeField]
        private TMP_Text beatAndTimeText = null!;

        private CompositeDisposable? disposables;

        public void Bind(BpmGroupViewModel targetViewModel, int index)
        {
            destroyCancellationToken.ThrowIfCancellationRequested();

            TryReleaseBind();
            disposables = new CompositeDisposable();

            itemNumberText.text = $"#{index + 1}";
            targetViewModel.SelectedBpmItemIndex
                .Subscribe(i => ledRawImage.enabled = i == index)
                .AddTo(disposables);
            targetViewModel.BpmGroupDataChangedSubject
                .Subscribe(changedItemIndex =>
                {
                    if (changedItemIndex > index)
                        return;

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

                    beatAndTimeText.text = $"{timePart}\n{beatPart}";
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

        void OnDestroy()
        {
            TryReleaseBind();
        }

        public void TryReleaseBind()
        {
            disposables?.Dispose();
            disposables = null;
        }
    }
}
