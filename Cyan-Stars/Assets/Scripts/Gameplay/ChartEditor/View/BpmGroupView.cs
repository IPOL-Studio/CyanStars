#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils.SelectableUI;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class BpmGroupView : BasePopupView<BpmGroupViewModel>
    {
        [Header("列表子 View")]
        [SerializeField]
        private GameObject bpmListItemPrefab = null!;

        [SerializeField]
        private RectTransform itemContentTransform = null!;

        [SerializeField]
        private GameObject bpmGroupListGameObject = null!;

        [SerializeField]
        private Button addBpmItemButton = null!;


        [Header("主 View")]
        [SerializeField]
        private GameObject timelineGameObject = null!;

        [SerializeField]
        private GameObject detailFrameGameObject = null!;

        [SerializeField]
        private TMP_Text numberText = null!;

        [SerializeField]
        private TMP_InputField bpmInputField = null!;

        [SerializeField]
        private Button testBpmButton = null!;

        [SerializeField]
        private TMP_InputField startBeatField1 = null!;

        [SerializeField]
        private TMP_InputField startBeatField2 = null!;

        [SerializeField]
        private TMP_InputField startBeatField3 = null!;

        [SerializeField]
        private Button deleteItemButton = null!;

        [SerializeField]
        private Toggle audioToggle = null!;

        [SerializeField]
        private Button testButton = null!;


        private ReadOnlyReactiveProperty<bool> listVisibility = null!;
        private Stack<BpmGroupListItemView> disabledListItemViews = new Stack<BpmGroupListItemView>(); // 当做对象池来用
        private SelectableStateObserver? deleteItemButtonSelectableStateObserver;


        public override void Bind(BpmGroupViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            deleteItemButtonSelectableStateObserver = deleteItemButton.GetComponent<SelectableStateObserver>();

            listVisibility = Observable.CombineLatest(
                    ViewModel.IsMultiBpmItemMode,
                    ViewModel.ChartPackData,
                    (isMultiBpmItemMode, chartPackData) => isMultiBpmItemMode || (chartPackData?.BpmGroup.Count ?? 0) == 0
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(this);

            // 创建列表 VM 和 V 并绑定
            for (int i = 0; i < ViewModel.BpmItems.Count; i++)
            {
                var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                go.GetComponent<BpmGroupListItemView>().Bind(ViewModel, i);
                go.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
            }

            ViewModel.BpmItems.ObserveChanged()
                .Subscribe(e =>
                {
                    // 将所有的 -1 视为不参与比较，然后从所有参与比较的数中取最小值，刷新其与其后的所有 index
                    // 显然如果观察到列表变化，两者都小于 0 的情况是不可能的
                    int index;
                    if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
                        index = Math.Min(e.OldStartingIndex, e.NewStartingIndex);
                    else if (e.OldStartingIndex >= 0 && e.NewStartingIndex < 0)
                        index = e.OldStartingIndex;
                    else if (e.OldStartingIndex < 0 && e.NewStartingIndex >= 0)
                        index = e.NewStartingIndex;
                    else
                        throw new Exception("列表发生变化但变化 index 均小于 0。");
                    RebuildBpmItemViews(index);
                })
                .AddTo(this);

            // VM -> V 绑定
            listVisibility
                .Subscribe(visible => bpmGroupListGameObject.SetActive(visible))
                .AddTo(this);
            ViewModel.SelectedBpmItem
                .Subscribe(item =>
                {
                    detailFrameGameObject.SetActive(item != null);
                    bpmInputField.SetTextWithoutNotify(item?.Bpm.ToString(CultureInfo.InvariantCulture));
                    startBeatField1.SetTextWithoutNotify(item?.StartBeat.IntegerPart.ToString());
                    startBeatField2.SetTextWithoutNotify(item?.StartBeat.Numerator.ToString());
                    startBeatField3.SetTextWithoutNotify(item?.StartBeat.Denominator.ToString());
                })
                .AddTo(this);

            ViewModel.SelectedBpmItemIndex
                .Subscribe(index =>
                {
                    // 修改标题 number
                    numberText.text = index != null ? $"#{(index + 1).ToString()}" : "";

                    // 如果是首个元素，则不允许修改 startBeat（锁定为 [0,0,1] ）
                    startBeatField1.interactable = index != 0;
                    startBeatField2.interactable = index != 0;
                    startBeatField3.interactable = index != 0;

                    // 如果是首个元素，则不允许删除
                    if (deleteItemButtonSelectableStateObserver != null)
                        // 挂载了 SelectableStateObserver，用 SelectableStateObserver 的方法设置并刷新视觉效果
                        deleteItemButtonSelectableStateObserver.SetInteractable(index != 0);
                    else
                        // 没有挂载 SelectableStateObserver，用 Unity 原生方法设置
                        deleteItemButton.interactable = index != 0;
                })
                .AddTo(this);

            // V -> VM 绑定
            addBpmItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddBpmItem())
                .AddTo(this);
            bpmInputField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetBpm)
                .AddTo(this);
            startBeatField1
                .OnEndEditAsObservable()
                .Subscribe(_ => SetBeat())
                .AddTo(this);
            startBeatField2
                .OnEndEditAsObservable()
                .Subscribe(_ => SetBeat())
                .AddTo(this);
            startBeatField3
                .OnEndEditAsObservable()
                .Subscribe(_ => SetBeat())
                .AddTo(this);
            deleteItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.DeleteSelectedBpmItem())
                .AddTo(this);
        }

        private void SetBeat()
        {
            ViewModel.SetBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
        }

        private (GameObject go, BpmGroupListItemView view) GetOrCreateItemView()
        {
            if (disabledListItemViews.TryPop(out var view))
            {
                view.gameObject.SetActive(true);
                return (view.gameObject, view);
            }
            else
            {
                var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                return (go, go.GetComponent<BpmGroupListItemView>());
            }
        }

        private void RebuildBpmItemViews(int afterIndex = 0)
        {
            // TODO: itemContentTransform 末尾有一个添加按钮，故 -2，考虑下次将所有 item 单独移到一个 Frame 内
            for (int i = itemContentTransform.childCount - 2; i >= afterIndex; i--)
            {
                var item = itemContentTransform.GetChild(i);
                if (!item.gameObject.activeSelf)
                    continue;
                var itemView = item.GetComponent<BpmGroupListItemView>();
                itemView.TryReleaseBind();
                disabledListItemViews.Push(item.GetComponent<BpmGroupListItemView>());
                item.gameObject.SetActive(false);
            }

            for (int i = afterIndex; i < ViewModel.BpmItems.Count; i++)
            {
                var (go, item) = GetOrCreateItemView();
                item.Bind(ViewModel, i);
                item.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
            }
        }
    }
}
