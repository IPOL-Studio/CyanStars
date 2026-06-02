#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.SelectableUI;
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

        public void Bind(BpmGroupViewModel targetViewModel, int index)
        {
            destroyCancellationToken.ThrowIfCancellationRequested();

            TryReleaseBind();
            disposables = new CompositeDisposable();

            itemNumberText.text = $"#{index + 1}";
            beatAndTimeText.text = GetDetailText(targetViewModel, index);

            SelectableStateObserver? selectableStateObserver = itemToggle.GetComponent<SelectableStateObserver>();
            targetViewModel.SelectedBpmItemIndex
                .Subscribe(i =>
                {
                    itemToggle.SetIsOnWithoutNotify(i == index);
                    selectableStateObserver?.RefreshToggleIson();
                })
                .AddTo(disposables);
            targetViewModel.BpmGroupDataChangedSubject
                .Subscribe(changedItemIndex =>
                {
                    if (changedItemIndex > index)
                        return;

                    beatAndTimeText.text = GetDetailText(targetViewModel, index);
                })
                .AddTo(disposables);

            // TODO: 后续改为 RadioButton 控制 Toggle 状态
            itemToggle
                .OnValueChangedAsObservable()
                .Subscribe((targetViewModel, index, itemToggle, selectableStateObserver), static (isOn, state) =>
                {
                    if (isOn)
                        state.targetViewModel.SelectBpmItem(state.targetViewModel.BpmItems[state.index]);
                    else if (state.targetViewModel.SelectedBpmItemIndex.CurrentValue == state.index)
                    {
                        // 拦截用户的取消勾选
                        state.itemToggle.SetIsOnWithoutNotify(true);
                        state.selectableStateObserver.RefreshToggleIson();
                    }
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

            itemToggle.group = null;
            itemToggle.isOn = false;
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
