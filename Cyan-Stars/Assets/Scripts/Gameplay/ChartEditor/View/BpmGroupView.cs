#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;

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
        private TMP_Text detailCountText = null!;

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


        public override void Bind(BpmGroupViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

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
            ViewModel.CanvasVisible
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            ViewModel.ListVisible
                .Subscribe(visible => bpmGroupListGameObject.SetActive(visible))
                .AddTo(this);
            ViewModel.SelectedBpmItem
                .Subscribe(selectedItem => detailFrameGameObject.SetActive(selectedItem != null))
                .AddTo(this);

            ViewModel.BPMText
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
        }

        protected override void OnDestroy()
        {
        }
    }
}
