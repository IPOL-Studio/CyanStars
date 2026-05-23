#nullable enable

using System.Threading.Tasks;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class MusicVersionView : BasePopupView<MusicVersionViewModel>
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

        [Header("主 View")]
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


        protected override async Task CloseCanvas()
        {
            await base.CloseCanvas();
            ViewModel.LoadAudio();
        }

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

            // VM -> V 绑定
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
            addListItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddMusicVersionItem())
                .AddTo(this);
            musicTitleField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetTitle)
                .AddTo(this);
            importMusicButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ImportAudioFile())
                .AddTo(this);
            minusOffsetButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.MinusOffset())
                .AddTo(this);
            offsetField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetOffset)
                .AddTo(this);
            addOffsetButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.AddOffset())
                .AddTo(this);
            testOffsetButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.TestOffset())
                .AddTo(this);
            deleteItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.DeleteItem())
                .AddTo(this);
            cloneItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.CloneItem())
                .AddTo(this);
            moveUpItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.MoveUpItem())
                .AddTo(this);
            moveDownItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.MoveDownItem())
                .AddTo(this);
            topItemButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.TopItem())
                .AddTo(this);
        }
    }
}
