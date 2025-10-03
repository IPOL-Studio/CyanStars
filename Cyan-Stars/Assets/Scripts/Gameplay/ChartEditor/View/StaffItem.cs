using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class StaffItem : BaseView
    {
        private bool isInit = false;
        private MusicVersionData musicVersionData;
        private KeyValuePair<string, List<string>> staffItemData;

        [SerializeField]
        private TMP_InputField staffIdField;

        [SerializeField]
        private TMP_InputField staffJobField;

        [SerializeField]
        private TMP_Dropdown staffJobDropdown;

        [SerializeField]
        private Button deleteItemButton;


        public void InitDataAndBind(EditorModel editorModel, MusicVersionData musicVersionData,
            KeyValuePair<string, List<string>> staffItemData)
        {
            isInit = true;
            this.musicVersionData = musicVersionData;
            this.staffItemData = staffItemData;
            Bind(editorModel);
        }

        public override void Bind(EditorModel editorModel)
        {
            if (!isInit)
            {
                throw new Exception("StaffItem: 未初始化数据");
            }

            base.Bind(editorModel);

            staffIdField.onEndEdit.RemoveAllListeners();
            staffIdField.onEndEdit.AddListener((newName) =>
            {
                Model.UpdateStaffItem(musicVersionData, staffItemData, newName, staffJobField.text);
            });
            staffJobField.onEndEdit.RemoveAllListeners();
            staffJobField.onEndEdit.AddListener((newJob) =>
            {
                Model.UpdateStaffItem(musicVersionData, staffItemData, staffIdField.text, newJob);
            });
            deleteItemButton.onClick.RemoveAllListeners();
            deleteItemButton.onClick.AddListener(() => { Model.DeleteStaffItem(musicVersionData, staffItemData); });

            RefreshUI();
        }

        private void RefreshUI()
        {
            staffIdField.text = staffItemData.Key;
            staffJobField.text = string.Join("/", staffItemData.Value);
        }
    }
}
