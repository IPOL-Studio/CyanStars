#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class MusicVersionView : BaseView<MusicVersionViewModel>
    {
        [Header("列表子 View")]
        [SerializeField]
        private GameObject musicListItemPrefab = null!;

        [SerializeField]
        private RectTransform musicListContentTransform = null!;

        [SerializeField]
        private GameObject musicListObject = null!;

        [SerializeField]
        private Button addListItemButton = null!;


        [Header("Staffs 子 View")]
        [SerializeField]
        private RectTransform staffsContentTransform = null!;

        [SerializeField]
        private Button addStaffButton = null!;


        [Header("主 View")]
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeButton = null!;

        [SerializeField]
        private GameObject detailObject = null!;

        [SerializeField]
        private TMP_InputField musicTitleField = null!;

        [SerializeField]
        private TMP_Text musicFilePathText = null!;

        [SerializeField]
        private Button importMusicButton = null!;

        [SerializeField]
        private TMP_InputField offsetField = null!;

        [SerializeField]
        private Button minusOffsetButton = null!;

        [SerializeField]
        private Button addOffsetButton = null!;

        [SerializeField]
        private Button testOffsetButton = null!;

        [SerializeField]
        private Button deleteItemButton = null!;

        [SerializeField]
        private Button cloneItemButton = null!;

        [SerializeField]
        private Button moveUpItemButton = null!;

        [SerializeField]
        private Button moveDownItemButton = null!;

        [SerializeField]
        private Button topItemButton = null!;


        public override void Bind(MusicVersionViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            // 创建列表 VM 和 V 并绑定
            foreach (var listItemViewModel in ViewModel.ListItems)
            {
                var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                go.GetComponent<MusicVersionListItemView>().Bind(listItemViewModel);
                go.transform.SetSiblingIndex(musicListContentTransform.childCount - 2); // 插入到倒数第二个位置，保持最后一个添加按钮在末尾
            }

            ViewModel.ListItems.ObserveAdd()
                .Subscribe(e =>
                {
                    var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                    go.GetComponent<MusicVersionListItemView>().Bind(e.Value);
                    go.transform.SetSiblingIndex(e.Index);
                })
                .AddTo(this);

            ViewModel.ListItems.ObserveRemove()
                .Subscribe(e =>
                {
                    var itemToRemove = musicListContentTransform.GetChild(e.Index);
                    Destroy(itemToRemove.gameObject);
                })
                .AddTo(this);

            ViewModel.ListItems.ObserveMove()
                .Subscribe(e =>
                {
                    var itemToMove = musicListContentTransform.GetChild(e.OldIndex);
                    itemToMove.SetSiblingIndex(e.NewIndex);
                })
                .AddTo(this);

            ViewModel.ListItems.ObserveReplace()
                .Subscribe(e =>
                {
                    var itemToRemove = musicListContentTransform.GetChild(e.Index);
                    Destroy(itemToRemove.gameObject);

                    {
                        var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                        go.GetComponent<MusicVersionListItemView>().Bind(e.NewValue);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                })
                .AddTo(this);

            ViewModel.ListItems.ObserveReset()
                .Subscribe(e =>
                {
                    for (int i = musicListContentTransform.childCount - 2; i >= 0; i--)
                    {
                        Destroy(musicListContentTransform.GetChild(i).gameObject);
                    }

                    foreach (var viewModelItem in ViewModel.ListItems)
                    {
                        var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                        go.GetComponent<MusicVersionListItemView>().Bind(viewModelItem);
                        go.transform.SetSiblingIndex(musicListContentTransform.childCount - 2);
                    }
                })
                .AddTo(this);

            // VM -> V 绑定
            ViewModel.CanvasVisibility
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            ViewModel.ListVisibility
                .Subscribe(visible => musicListObject.SetActive(visible))
                .AddTo(this);
            ViewModel.DetailVisibility
                .Subscribe(visible => detailObject.SetActive(visible))
                .AddTo(this);
            ViewModel.DetailTitle
                .Subscribe(title => musicTitleField.text = title)
                .AddTo(this);
            ViewModel.DetailAudioFilePath
                .Subscribe(text => musicFilePathText.text = text)
                .AddTo(this);
            ViewModel.DetailOffset
                .Subscribe(text => offsetField.text = text)
                .AddTo(this);

            // V -> MV 绑定
            addListItemButton.onClick.AddListener(ViewModel.AddMusicVersionItem);
            closeButton.onClick.AddListener(ViewModel.CloseCanvas);
            musicTitleField.onEndEdit.AddListener(ViewModel.SetTitle);
            importMusicButton.onClick.AddListener(ViewModel.ImportAudioFile);
            minusOffsetButton.onClick.AddListener(ViewModel.MinusOffset);
            offsetField.onEndEdit.AddListener(ViewModel.SetOffset);
            addOffsetButton.onClick.AddListener(ViewModel.AddOffset);
            testOffsetButton.onClick.AddListener(ViewModel.TestOffset);
            addStaffButton.onClick.AddListener(ViewModel.AddStaffItem);
            deleteItemButton.onClick.AddListener(ViewModel.DeleteItem);
            cloneItemButton.onClick.AddListener(ViewModel.CloneItem);
            moveUpItemButton.onClick.AddListener(ViewModel.MoveUpItem);
            moveDownItemButton.onClick.AddListener(ViewModel.MoveDownItem);
            topItemButton.onClick.AddListener(ViewModel.TopItem);
        }

        protected override void OnDestroy()
        {
            addListItemButton.onClick.RemoveListener(ViewModel.AddMusicVersionItem);
            closeButton.onClick.RemoveListener(ViewModel.CloseCanvas);
            musicTitleField.onEndEdit.RemoveListener(ViewModel.SetTitle);
            importMusicButton.onClick.RemoveListener(ViewModel.ImportAudioFile);
            minusOffsetButton.onClick.RemoveListener(ViewModel.MinusOffset);
            offsetField.onEndEdit.RemoveListener(ViewModel.SetOffset);
            addOffsetButton.onClick.RemoveListener(ViewModel.AddOffset);
            testOffsetButton.onClick.RemoveListener(ViewModel.TestOffset);
            addStaffButton.onClick.RemoveListener(ViewModel.AddStaffItem);
            deleteItemButton.onClick.RemoveListener(ViewModel.DeleteItem);
            cloneItemButton.onClick.RemoveListener(ViewModel.CloneItem);
            moveUpItemButton.onClick.RemoveListener(ViewModel.MoveUpItem);
            moveDownItemButton.onClick.RemoveListener(ViewModel.MoveDownItem);
            topItemButton.onClick.RemoveListener(ViewModel.TopItem);
        }
    }
}
