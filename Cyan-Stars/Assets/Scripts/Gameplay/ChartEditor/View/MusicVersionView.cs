#nullable enable

using System;
using System.Collections.Specialized;
using CyanStars.Gameplay.ChartEditor.ViewModel;
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
        private Button testOffsetButton = null!; // TODO

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

            // 初始化各部分值，创建列表 VM 和 V 并绑定
            canvas.enabled = ViewModel.CanvasVisibility.Value;
            musicListObject.SetActive(ViewModel.ListVisibility.Value);
            foreach (var listItemViewModel in ViewModel.ListItems)
            {
                var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                go.GetComponentInChildren<MusicVersionListItemView>().Bind(listItemViewModel);
                go.transform.SetSiblingIndex(musicListContentTransform.childCount - 2);
            }

            detailObject.SetActive(ViewModel.DetailVisibility.Value);
            musicTitleField.text = ViewModel.DetailTitle.Value;
            musicFilePathText.text = ViewModel.DetailAudioFilePath.Value;
            offsetField.text = ViewModel.DetailOffset.Value;

            // VM -> V 绑定
            ViewModel.CanvasVisibility.OnValueChanged += SetCanvasVisibility;
            ViewModel.ListVisibility.OnValueChanged += SetListObjectVisibility;
            ViewModel.ListItems.CollectionChanged += RefreshListItems;
            ViewModel.DetailVisibility.OnValueChanged += SetDetailObjectVisibility;
            ViewModel.DetailTitle.OnValueChanged += SetDetailTitleText;
            ViewModel.DetailAudioFilePath.OnValueChanged += SetFilePathText;
            ViewModel.DetailOffset.OnValueChanged += SetOffsetText;

            // V -> MV 绑定
            closeButton.onClick.AddListener(ViewModel.CloseCanvas);
            musicTitleField.onEndEdit.AddListener(ViewModel.SetTitle);
            importMusicButton.onClick.AddListener(ViewModel.ImportAudioFile);
            offsetField.onEndEdit.AddListener(ViewModel.SetOffset);
            addStaffButton.onClick.AddListener(ViewModel.AddStaffItem);
        }


        private void SetCanvasVisibility(bool visible)
        {
            canvas.enabled = visible;
        }

        private void SetListObjectVisibility(bool visible)
        {
            musicListObject.SetActive(visible);
        }

        /// <summary>
        /// 生成并绑定/销毁/重排序 ListItemGameObjects
        /// </summary>
        private void RefreshListItems(object obj, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var itemViewModel = (MusicVersionListItemViewModel)(e.NewItems[i]);
                        var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                        go.GetComponentInChildren<MusicVersionListItemView>().Bind(itemViewModel);
                        go.transform.SetSiblingIndex(e.NewStartingIndex + i);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    var itemToMove = musicListContentTransform.GetChild(e.OldStartingIndex);
                    itemToMove.SetSiblingIndex(e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = e.OldItems.Count - 1; i >= 0; i--)
                    {
                        var itemToRemove = musicListContentTransform.GetChild(e.OldStartingIndex + i);
                        Destroy(itemToRemove.gameObject);
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    for (int i = e.OldItems.Count - 1; i >= 0; i--)
                    {
                        var itemToRemove = musicListContentTransform.GetChild(e.OldStartingIndex + i);
                        Destroy(itemToRemove.gameObject);
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var itemViewModel = (MusicVersionListItemViewModel)(e.NewItems[i]);
                        var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                        go.GetComponentInChildren<MusicVersionListItemView>().Bind(itemViewModel);
                        go.transform.SetSiblingIndex(e.NewStartingIndex + i);
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    for (int i = musicListContentTransform.childCount - 2; i >= 0; i--)
                    {
                        Destroy(musicListContentTransform.GetChild(i).gameObject);
                    }

                    foreach (var listItemViewModel in ViewModel.ListItems)
                    {
                        var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                        go.GetComponentInChildren<MusicVersionListItemView>().Bind(listItemViewModel);
                        go.transform.SetSiblingIndex(musicListContentTransform.childCount - 2);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetDetailObjectVisibility(bool visible)
        {
            detailObject.SetActive(visible);
        }

        private void SetDetailTitleText(string title)
        {
            musicTitleField.text = title;
        }

        private void SetFilePathText(string text)
        {
            musicFilePathText.text = text;
        }

        private void SetOffsetText(string text)
        {
            offsetField.text = text;
        }


        protected override void OnDestroy()
        {
            ViewModel.CanvasVisibility.OnValueChanged -= SetCanvasVisibility;
            ViewModel.ListVisibility.OnValueChanged -= SetListObjectVisibility;
            ViewModel.ListItems.CollectionChanged -= RefreshListItems;
            ViewModel.DetailVisibility.OnValueChanged -= SetDetailObjectVisibility;
            ViewModel.DetailTitle.OnValueChanged -= SetDetailTitleText;
            ViewModel.DetailAudioFilePath.OnValueChanged -= SetFilePathText;
            ViewModel.DetailOffset.OnValueChanged -= SetOffsetText;

            closeButton.onClick.RemoveListener(ViewModel.CloseCanvas);
            musicTitleField.onEndEdit.RemoveListener(ViewModel.SetTitle);
            importMusicButton.onClick.RemoveListener(ViewModel.ImportAudioFile);
            offsetField.onEndEdit.RemoveListener(ViewModel.SetOffset);
            addStaffButton.onClick.RemoveListener(ViewModel.AddStaffItem);
        }
    }
}
