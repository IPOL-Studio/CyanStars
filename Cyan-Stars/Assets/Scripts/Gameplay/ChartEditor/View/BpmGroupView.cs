#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.ViewModel;
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


        private readonly ReactiveProperty<bool> CanvasVisibility = new ReactiveProperty<bool>(false);
        private ReadOnlyReactiveProperty<bool> listVisibility = null!;

        private Stack<BpmGroupListItemView> disabledListItemViews = new Stack<BpmGroupListItemView>();


        public override void Bind(BpmGroupViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            listVisibility = Observable.CombineLatest(
                    ViewModel.IsSimplificationMode,
                    ViewModel.ChartPackData,
                    (isSimple, chartPackData) => !isSimple || (chartPackData?.BpmGroup.Count ?? 0) == 0
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

            ViewModel.BpmItems.ObserveAdd()
                .Subscribe(e =>
                {
                    var (go, itemView) = GetOrCreateItemView();
                    itemView.Bind(ViewModel, e.Index);
                    go.transform.SetSiblingIndex(e.Index);
                })
                .AddTo(this);
            ViewModel.BpmItems.ObserveRemove()
                .Subscribe(e =>
                {
                    var itemToRemove = itemContentTransform.GetChild(e.Index);
                    var itemView = itemToRemove.GetComponent<BpmGroupListItemView>();
                    itemView.TryReleaseBind();
                    disabledListItemViews.Push(itemView);
                    itemToRemove.gameObject.SetActive(false);
                })
                .AddTo(this);
            ViewModel.BpmItems.ObserveMove()
                .Subscribe(e =>
                {
                    var itemToMove = itemContentTransform.GetChild(e.OldIndex);
                    itemToMove.SetSiblingIndex(e.NewIndex);
                    itemToMove.GetComponent<BpmGroupListItemView>().Bind(ViewModel, e.NewIndex);
                })
                .AddTo(this);
            ViewModel.BpmItems.ObserveReplace()
                .Subscribe(e =>
                {
                    var trans = itemContentTransform.GetChild(e.Index);
                    trans.GetComponent<BpmGroupListItemView>().Bind(ViewModel, e.Index);
                    trans.SetSiblingIndex(e.Index);
                })
                .AddTo(this);
            ViewModel.BpmItems.ObserveReset()
                .Subscribe(e =>
                {
                    for (int i = itemContentTransform.childCount - 2; i >= 0; i--)
                    {
                        var item = itemContentTransform.GetChild(i);
                        disabledListItemViews.Push(item.GetComponent<BpmGroupListItemView>());
                        item.gameObject.SetActive(false);
                    }

                    for (int i = 0; i < ViewModel.BpmItems.Count; i++)
                    {
                        var (go, item) = GetOrCreateItemView();
                        item.Bind(ViewModel, i);
                        item.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
                    }
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
                    bpmInputField.SetTextWithoutNotify(item?.Bpm.ToString());
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
    }
}
