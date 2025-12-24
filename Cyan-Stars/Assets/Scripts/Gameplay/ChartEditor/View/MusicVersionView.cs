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
        [Header("子 View")]
        [SerializeField]
        private GameObject musicListItemPrefab = null!;

        [SerializeField]
        private RectTransform musicListContentTransform = null!;

        [SerializeField]
        private GameObject musicListObject = null!;

        [SerializeField]
        private GameObject detailObject = null!;

        [Header("主 View")]
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeButton = null!;

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

            musicListObject.SetActive(ViewModel.ListVisibility.Value);
            foreach (var listItemViewModel in ViewModel.ListItems)
            {
                var go = Instantiate(musicListItemPrefab, musicListContentTransform);
                go.GetComponentInChildren<MusicVersionListItemView>().Bind(listItemViewModel);
                go.transform.SetSiblingIndex(musicListContentTransform.childCount - 2);
            }

            detailObject.SetActive(ViewModel.DetailVisibility.Value);
            canvas.enabled = ViewModel.CanvasVisibility.Value;


            ViewModel.ListVisibility.OnValueChanged += SetListObjectVisibility;
            ViewModel.ListItems.CollectionChanged += RefreshListItems;
            ViewModel.DetailVisibility.OnValueChanged += SetDetailObjectVisibility;
            ViewModel.CanvasVisibility.OnValueChanged += SetCanvasVisibility;


            closeButton.onClick.AddListener(ViewModel.CloseCanvas);
        }

        private void SetListObjectVisibility(bool visible)
        {
            musicListObject.SetActive(visible);
        }

        private void SetDetailObjectVisibility(bool visible)
        {
            detailObject.SetActive(visible);
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

        private void SetCanvasVisibility(bool visible)
        {
            canvas.enabled = visible;
        }

        protected override void OnDestroy()
        {
            ViewModel.ListItems.CollectionChanged -= RefreshListItems;
        }
    }
}
