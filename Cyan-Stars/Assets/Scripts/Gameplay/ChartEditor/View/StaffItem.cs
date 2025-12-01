#nullable enable

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
    public class StaffItem : BaseView
    {
        private bool isInit = false;
        private int staffItemIndex;
        private KeyValuePair<string, List<string>> staffItemData;

        [SerializeField]
        private TMP_InputField staffIdField = null!;

        [SerializeField]
        private TMP_InputField staffJobField = null!;

        [SerializeField]
        private TMP_Dropdown staffJobDropdown = null!;

        [SerializeField]
        private Button deleteItemButton = null!;


        public void InitAndBind(ChartEditorModel chartEditorModel, int staffItemIndex)
        {
            isInit = true;
            Bind(chartEditorModel);

            if (Model.SelectedMusicVersionItemIndex == null)
            {
                throw new NullReferenceException("StaffItem：未选中音乐版本，无法绑定");
            }

            this.staffItemIndex = staffItemIndex;
            MusicVersionData musicVersionData = Model.MusicVersionDatas[(int)Model.SelectedMusicVersionItemIndex];
            staffItemData = musicVersionData.Staffs.ElementAt(this.staffItemIndex);

            RefreshUI();
        }

        public override void Bind(ChartEditorModel chartEditorModel)
        {
            if (!isInit)
            {
                throw new Exception("StaffItem: 未初始化数据");
            }

            base.Bind(chartEditorModel);

            Model.OnMusicVersionDataChanged -= RefreshUI;
            Model.OnMusicVersionDataChanged += RefreshUI;
            Model.OnSelectedMusicVersionItemChanged -= RefreshUI;
            Model.OnSelectedMusicVersionItemChanged += RefreshUI;

            staffIdField.onEndEdit.RemoveAllListeners();
            staffIdField.onEndEdit.AddListener((newName) =>
            {
                Model.UpdateStaffItem(staffItemData, newName, staffJobField.text);
            });
            staffJobField.onEndEdit.RemoveAllListeners();
            staffJobField.onEndEdit.AddListener((newJob) =>
            {
                Model.UpdateStaffItem(staffItemData, staffIdField.text, newJob);
            });
            deleteItemButton.onClick.RemoveAllListeners();
            deleteItemButton.onClick.AddListener(() => { Model.DeleteStaffItem(staffItemData); });
        }

        private void RefreshUI()
        {
            staffIdField.text = staffItemData.Key;
            staffJobField.text = string.Join("/", staffItemData.Value);
        }
    }
}
