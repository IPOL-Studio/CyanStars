using System;
using System.Collections.Generic;
using System.Linq;
using CyanStars.Chart;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class MusicVersionItem : BaseView
    {
        private const int OffsetStep = 10;


        private bool isInit = false;
        private MusicVersionData musicVersionData;
        private List<KeyValuePair<string, List<string>>> staffItems = new List<KeyValuePair<string, List<string>>>();


        [SerializeField]
        private TMP_InputField titleField;

        [SerializeField]
        private TMP_Text audioFilePath;

        [SerializeField]
        private Button importMusicButton; //TODO: 从操作系统导入音频文件路径

        [SerializeField]
        private TMP_InputField offsetField;

        [SerializeField]
        private Button subOffsetButton;

        [SerializeField]
        private Button addOffsetButton;

        [SerializeField]
        private Button testOffsetButton;

        [SerializeField]
        private GameObject staffInfoFrameObject;

        [SerializeField]
        private GameObject staffItemPrefab;

        [SerializeField]
        private Button addStaffItemButton;

        [SerializeField]
        private Button deleteMusicVersionItemButton;

        [SerializeField]
        private Button copyMusicVersionItemButton;

        [SerializeField]
        private Button topMusicVersionItemButton;


        public void InitDataAndBind(ChartEditorModel chartEditorModel, MusicVersionData musicVersionData)
        {
            isInit = true;
            this.musicVersionData = musicVersionData;
            staffItems = musicVersionData.Staffs.ToList();
            Bind(chartEditorModel);
        }

        public override void Bind(ChartEditorModel chartEditorModel)
        {
            if (!isInit)
            {
                throw new Exception("MusicVersionItem: 未初始化数据");
            }

            base.Bind(chartEditorModel);

            titleField.onEndEdit.RemoveAllListeners();
            titleField.onEndEdit.AddListener((text) => { Model.UpdateMusicVersionTitle(musicVersionData, text); });
            offsetField.onEndEdit.RemoveAllListeners();
            offsetField.onEndEdit.AddListener((text) => { Model.UpdateMusicVersionOffset(musicVersionData, text); });
            subOffsetButton.onClick.RemoveAllListeners();
            subOffsetButton.onClick.AddListener(() => { Model.AddMusicVersionOffsetValue(musicVersionData, -OffsetStep); });
            addOffsetButton.onClick.RemoveAllListeners();
            addOffsetButton.onClick.AddListener(() => { Model.AddMusicVersionOffsetValue(musicVersionData, OffsetStep); });
            addStaffItemButton.onClick.RemoveAllListeners();
            addStaffItemButton.onClick.AddListener(() => { Model.AddStaffItem(musicVersionData); });
            deleteMusicVersionItemButton.onClick.RemoveAllListeners();
            deleteMusicVersionItemButton.onClick.AddListener(() => { Model.DeleteMusicVersionItem(musicVersionData); });
            copyMusicVersionItemButton.onClick.RemoveAllListeners();
            copyMusicVersionItemButton.onClick.AddListener(() => { Model.CopyMusicVersionItem(musicVersionData); });
            topMusicVersionItemButton.onClick.RemoveAllListeners();
            topMusicVersionItemButton.onClick.AddListener(() => { Model.ApplyMusicVersionItem(musicVersionData); });
            RefreshUI();
        }

        private void RefreshUI()
        {
            titleField.text = musicVersionData.VersionTitle;
            audioFilePath.text = musicVersionData.AudioFilePath;
            offsetField.text = musicVersionData.Offset.ToString();

            // 删除多余元素
            StaffItem[] items = staffInfoFrameObject.GetComponentsInChildren<StaffItem>();
            for (int i = items.Length - 1; i >= musicVersionData.Staffs.Count; i--)
            {
                Destroy(items[i].gameObject);
            }

            // 刷新已有元素的内容
            items = staffInfoFrameObject.GetComponentsInChildren<StaffItem>();
            for (int i = 0; i < items.Length; i++)
            {
                items[i].InitDataAndBind(Model, musicVersionData, staffItems[i]);
            }

            // 添加并刷新新元素
            for (int i = items.Length; i < musicVersionData.Staffs.Count; i++)
            {
                GameObject go = Instantiate(staffItemPrefab, staffInfoFrameObject.transform);
                go.transform.SetSiblingIndex(staffInfoFrameObject.transform.childCount - 3);
                go.GetComponent<StaffItem>().InitDataAndBind(Model, musicVersionData, staffItems[i]);
            }
        }
    }
}
