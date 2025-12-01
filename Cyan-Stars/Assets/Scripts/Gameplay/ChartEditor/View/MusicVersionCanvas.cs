using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class MusicVersionCanvas : BaseView
    {
        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private Button closeCanvasButton;

        [Header("列表区域")]
        [SerializeField]
        private GameObject listObject;

        [SerializeField]
        private GameObject contentObject;

        [SerializeField]
        private Button addItemButton;

        [SerializeField]
        private GameObject musicVersionItemPrefab;

        [Header("数据编辑区域")]
        [SerializeField]
        private GameObject dataAreaFrameObject;

        [SerializeField]
        private TMP_InputField titleField;

        [SerializeField]
        private TMP_Text filePathText;

        [SerializeField]
        private Button importFileButton;

        [SerializeField]
        private Button minusOffsetButton;

        [SerializeField]
        private TMP_InputField offsetField;

        [SerializeField]
        private Button addOffsetButton;

        [SerializeField]
        private Button testPlayButton;

        [SerializeField]
        private GameObject staffInfoAreaObject;

        [SerializeField]
        private Button addStaffItemButton;

        [SerializeField]
        private Button deleteMusicVersionButton;

        [SerializeField]
        private Button cloneMusicVersionButton;

        [SerializeField]
        private Button moveUpMusicVersionButton;

        [SerializeField]
        private Button moveDownMusicVersionButton;

        [SerializeField]
        private Button topMusicVersionButton;

        [SerializeField]
        private GameObject staffItemPrefab;


        private readonly List<MusicVersionItem> ListItems = new List<MusicVersionItem>();
        private readonly List<StaffItem> StaffItems = new List<StaffItem>();


        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            closeCanvasButton.onClick.AddListener(() => { Model.SetMusicVersionCanvasVisibleness(false); });
            addItemButton.onClick.AddListener(() => { Model.AddMusicVersionItem(); });

            titleField.onEndEdit.AddListener(Model.UpdateMusicVersionTitle);
            importFileButton.onClick.AddListener(() =>
            {
                if (Model.SelectedMusicVersionItemIndex != null)
                {
                    GameRoot.File.OpenLoadFilePathBrowser(
                        (musicFilePath) => { Model.ImportMusicFile(musicFilePath); },
                        title: "导入音频",
                        filters: new[] { GameRoot.File.AudioFilter });
                }
            });
            minusOffsetButton.onClick.AddListener(() =>
            {
                int? index = Model.SelectedMusicVersionItemIndex;
                if (index != null)
                {
                    Model.UpdateMusicVersionOffset((Model.MusicVersionDatas[(int)index].Offset - 10).ToString());
                }
            });
            offsetField.onEndEdit.AddListener(Model.UpdateMusicVersionOffset);
            addOffsetButton.onClick.AddListener(() =>
            {
                int? index = Model.SelectedMusicVersionItemIndex;
                if (index != null)
                {
                    Model.UpdateMusicVersionOffset((Model.MusicVersionDatas[(int)index].Offset + 10).ToString());
                }
            });
            testPlayButton.onClick.AddListener(() => { Debug.LogError("未实现"); }); //TODO

            addStaffItemButton.onClick.AddListener(Model.AddStaffItem);
            deleteMusicVersionButton.onClick.AddListener(Model.DeleteMusicVersionItem);
            cloneMusicVersionButton.onClick.AddListener(Model.CloneMusicVersionItem);
            moveUpMusicVersionButton.onClick.AddListener(Model.MoveUpMusicVersionItem);
            moveDownMusicVersionButton.onClick.AddListener(Model.MoveDownMusicVersionItem);
            topMusicVersionButton.onClick.AddListener(Model.TopMusicVersionItem);

            Model.OnMusicVersionDataChanged += RefreshUI;
            Model.OnSelectedMusicVersionItemChanged += RefreshUI;
            Model.OnMusicVersionCanvasVisiblenessChanged += RefreshUI;

            RefreshUI();
        }

        private void RefreshUI()
        {
            canvas.enabled = Model.MusicVersionCanvasVisibleness;

            // 简易模式下至少存在1个音乐版本，没有时自动补齐至1个
            if (Model.MusicVersionDatas.Count == 0 && Model.IsSimplification)
            {
                Model.AddMusicVersionItem();
            }

            // 如果不处于简易模式，或存在多个版本，则显示列表栏，并允许添加版本
            bool showList = !Model.IsSimplification || Model.MusicVersionDatas.Count > 1;
            listObject.SetActive(showList);
            if (showList)
            {
                // 删除列表栏多余 item
                for (int i = ListItems.Count - 1; i >= Model.MusicVersionDatas.Count; i--)
                {
                    Destroy(ListItems[i].gameObject);
                    ListItems.RemoveAt(i);
                }

                // 补齐列表栏 item
                for (int i = ListItems.Count; i < Model.MusicVersionDatas.Count; i++)
                {
                    GameObject go = Instantiate(musicVersionItemPrefab, contentObject.transform);
                    go.transform.SetSiblingIndex(contentObject.transform.childCount - 2); // 置于倒数第二个
                    MusicVersionItem item = go.GetComponent<MusicVersionItem>();
                    ListItems.Add(item);
                }

                // 重新初始化、绑定、刷新所有列表 item TODO：优化性能
                for (int i = 0; i < ListItems.Count; i++)
                {
                    ListItems[i].InitAndBind(Model, i);
                }

                // 刷新 UI 自动布局
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentObject.GetComponent<RectTransform>());
            }

            // 根据目前选中编辑的音乐版本显示数据
            bool selected = Model.SelectedMusicVersionItemIndex != null;
            dataAreaFrameObject.SetActive(selected);
            if (selected)
            {
                MusicVersionData data = Model.MusicVersionDatas[(int)Model.SelectedMusicVersionItemIndex];
                titleField.text = data.VersionTitle ?? "";
                filePathText.text = data.AudioFilePath ?? "";
                offsetField.text = data.Offset.ToString();

                // 删除多余的 Staff item
                for (int i = StaffItems.Count - 1; i >= data.Staffs.Count; i--)
                {
                    Destroy(StaffItems[i].gameObject);
                    StaffItems.RemoveAt(i);
                }

                // 补齐 Staff item
                for (int i = StaffItems.Count; i < data.Staffs.Count; i++)
                {
                    GameObject go = Instantiate(staffItemPrefab, staffInfoAreaObject.transform);
                    go.transform.SetSiblingIndex(staffInfoAreaObject.transform.childCount - 3); // 置于倒数第三个
                    StaffItem item = go.GetComponent<StaffItem>();
                    StaffItems.Add(item);
                }

                // 重新绑定并刷新 Staff item
                for (int i = 0; i < StaffItems.Count; i++)
                {
                    StaffItems[i].InitAndBind(Model, i);
                }
            }
        }

        private void OnDestroy()
        {
            Model.OnMusicVersionDataChanged -= RefreshUI;
            Model.OnMusicVersionCanvasVisiblenessChanged -= RefreshUI;
        }
    }
}
