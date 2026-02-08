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
    public class MusicVersionView : BaseView<MusicVersionViewModel>
    {
        [Header("列表子 View")]
        [SerializeField]
        private GameObject musicListItemPrefab = null!;

        [SerializeField]
        private RectTransform itemContentTransform = null!;

        [SerializeField]
        private GameObject musicListObject = null!;

        [SerializeField]
        private Button addListItemButton = null!;


        [Header("Staffs 子 View")]
        [SerializeField]
        private GameObject staffItemPrefab = null!;

        [SerializeField]
        private GameObject staffsContentFrameGameObject = null!;

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


        private readonly ReactiveProperty<bool> CanvasVisibility = new ReactiveProperty<bool>(false);
        private ReadOnlyReactiveProperty<bool> listVisibility = null!;
        private ReadOnlyReactiveProperty<bool> detailVisibility = null!;


        public override void Bind(MusicVersionViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            listVisibility = Observable
                .CombineLatest(
                    ViewModel.IsSimplificationMode,
                    ViewModel.ChartPackData
                        .Select(data => data.MusicVersions.ObserveCountChanged(notifyCurrentCount: true))
                        .Switch(),
                    ViewModel.SelectedMusicVersionData,
                    (isSimplificationMode, count, selectedData) =>
                        !(isSimplificationMode && count == 1 && selectedData != null)
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(this);
            detailVisibility =
                ViewModel.SelectedMusicVersionData
                    .Select(data => data != null)
                    .ToReadOnlyReactiveProperty()
                    .AddTo(this);


            // 创建列表 VM 和 V 并绑定
            foreach (var listItemViewModel in ViewModel.MusicListItems)
            {
                var go = Instantiate(musicListItemPrefab, itemContentTransform);
                go.GetComponent<MusicVersionListItemView>().Bind(listItemViewModel);
                go.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
            }

            ViewModel.MusicListItems.ObserveAdd()
                .Subscribe(e =>
                    {
                        var go = Instantiate(musicListItemPrefab, itemContentTransform);
                        go.GetComponent<MusicVersionListItemView>().Bind(e.Value.View);
                        go.transform.SetSiblingIndex(e.Index);
                    }
                )
                .AddTo(this);
            ViewModel.MusicListItems.ObserveRemove()
                .Subscribe(e =>
                    {
                        var itemToRemove = itemContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);
                    }
                )
                .AddTo(this);
            ViewModel.MusicListItems.ObserveMove()
                .Subscribe(e =>
                    {
                        var itemToMove = itemContentTransform.GetChild(e.OldIndex);
                        itemToMove.SetSiblingIndex(e.NewIndex);
                    }
                )
                .AddTo(this);
            ViewModel.MusicListItems.ObserveReplace()
                .Subscribe(e =>
                    {
                        var itemToRemove = itemContentTransform.GetChild(e.Index);
                        Destroy(itemToRemove.gameObject);

                        {
                            var go = Instantiate(musicListItemPrefab, itemContentTransform);
                            go.GetComponent<MusicVersionListItemView>().Bind(e.NewValue.View);
                            go.transform.SetSiblingIndex(e.Index);
                        }
                    }
                )
                .AddTo(this);
            ViewModel.MusicListItems.ObserveReset()
                .Subscribe(e =>
                    {
                        for (int i = itemContentTransform.childCount - 2; i >= 0; i--)
                        {
                            Destroy(itemContentTransform.GetChild(i).gameObject);
                        }

                        foreach (var viewModelItem in ViewModel.MusicListItems)
                        {
                            var go = Instantiate(musicListItemPrefab, itemContentTransform);
                            go.GetComponent<MusicVersionListItemView>().Bind(viewModelItem);
                            go.transform.SetSiblingIndex(itemContentTransform.childCount - 2);
                        }
                    }
                )
                .AddTo(this);


            // 创建 Staff VM 和 V 并绑定
            foreach (var staffItemViewModel in ViewModel.StaffItems)
            {
                var go = Instantiate(staffItemPrefab, staffsContentFrameGameObject.transform);
                go.GetComponent<MusicVersionStaffItemView>().Bind(staffItemViewModel);
                go.transform.SetSiblingIndex(staffsContentFrameGameObject.transform.childCount - 1);
            }

            // TODO: 有任何变化时直接全量刷新，之后再优化
            ViewModel.StaffItems.ObserveChanged()
                .Subscribe(e =>
                    {
                        for (int i = staffsContentFrameGameObject.transform.childCount - 1; i >= 0; i--)
                        {
                            Destroy(staffsContentFrameGameObject.transform.GetChild(i).gameObject);
                        }

                        foreach (var viewModelItem in ViewModel.StaffItems)
                        {
                            var go = Instantiate(staffItemPrefab, staffsContentFrameGameObject.transform);
                            go.GetComponent<MusicVersionStaffItemView>().Bind(viewModelItem);
                            go.transform.SetSiblingIndex(staffsContentFrameGameObject.transform.childCount - 1);
                        }
                    }
                )
                .AddTo(this);


            // VM -> V 绑定
            CanvasVisibility
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            listVisibility
                .Subscribe(visible => musicListObject.SetActive(visible))
                .AddTo(this);
            detailVisibility
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
            closeButton.onClick.AddListener(CloseCanvasAndLoadAudio);
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

        private void CloseCanvasAndLoadAudio()
        {
            if (!CanvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () => CanvasVisibility.Value = false,
                    () => CanvasVisibility.Value = true
                )
            );

            ViewModel.LoadAudio();
        }

        public void OpenCanvas()
        {
            if (CanvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () => CanvasVisibility.Value = true,
                    () => CanvasVisibility.Value = false
                )
            );
        }

        protected override void OnDestroy()
        {
            addListItemButton.onClick.RemoveListener(ViewModel.AddMusicVersionItem);
            closeButton.onClick.RemoveListener(CloseCanvasAndLoadAudio);
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
