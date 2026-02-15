#nullable enable

using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class BpmGroupView : BaseView<BpmGroupViewModel>
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
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

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
            foreach (var bpmGroupListItemViewModel in ViewModel.BpmListItems)
            {
                var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                go.GetComponent<BpmGroupListItemView>().Bind(bpmGroupListItemViewModel);
                go.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
            }

            ViewModel.BpmListItems.ObserveAdd()
                .Subscribe(e =>
                    {
                        var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                        go.GetComponent<BpmGroupListItemView>().Bind(e.Value.View);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                )
                .AddTo(this);
            ViewModel.BpmListItems.ObserveRemove()
                .Subscribe(e =>
                    {
                        var itemToRemove = itemContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);
                    }
                )
                .AddTo(this);
            ViewModel.BpmListItems.ObserveMove()
                .Subscribe(e =>
                    {
                        var itemToMove = itemContentTransform.GetChild(e.OldIndex);
                        itemToMove.SetSiblingIndex(e.NewIndex);
                    }
                )
                .AddTo(this);
            ViewModel.BpmListItems.ObserveReplace()
                .Subscribe(e =>
                    {
                        var itemToRemove = itemContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);

                        {
                            var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                            go.GetComponent<BpmGroupListItemView>().Bind(e.NewValue.View);
                            go.transform.SetSiblingIndex(e.Index);
                        }
                    }
                )
                .AddTo(this);
            ViewModel.BpmListItems.ObserveReset()
                .Subscribe(e =>
                    {
                        for (int i = itemContentTransform.childCount - 2; i >= 0; i--)
                        {
                            Destroy(itemContentTransform.GetChild(i).gameObject);
                        }

                        foreach (var viewModelItem in ViewModel.BpmListItems)
                        {
                            var go = Instantiate(bpmListItemPrefab, itemContentTransform);
                            go.GetComponent<BpmGroupListItemView>().Bind(viewModelItem);
                            go.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
                        }
                    }
                )
                .AddTo(this);

            // VM -> V 绑定
            CanvasVisibility
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            listVisibility
                .Subscribe(visible => bpmGroupListGameObject.SetActive(visible))
                .AddTo(this);
            ViewModel.SelectedBpmItem
                .Subscribe(selectedItem => detailFrameGameObject.SetActive(selectedItem != null))
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
                    }
                )
                .AddTo(this);
            ViewModel.BpmText
                .Subscribe(text => bpmInputField.text = text)
                .AddTo(this);
            ViewModel.StartBeatText1
                .Subscribe(text => startBeatField1.text = text)
                .AddTo(this);
            ViewModel.StartBeatText2
                .Subscribe(text => startBeatField2.text = text)
                .AddTo(this);
            ViewModel.StartBeatText3
                .Subscribe(text => startBeatField3.text = text)
                .AddTo(this);

            // V -> VM 绑定
            closeCanvasButton
                .OnClickAsObservable()
                .Subscribe(_ => CloseCanvas())
                .AddTo(this);
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

        private void CloseCanvas()
        {
            if (!CanvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                () => CanvasVisibility.Value = false,
                () => CanvasVisibility.Value = true
            );
        }

        public void OpenCanvas()
        {
            if (CanvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                () => CanvasVisibility.Value = true,
                () => CanvasVisibility.Value = false
            );
        }
    }
}
