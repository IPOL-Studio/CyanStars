using System;
using System.Collections.Generic;
using System.Linq;
using CyanStars.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class StaffItem : BaseView
    {
        private bool isInit = false;
        private string staffId;
        private List<string> staffJobs;

        [SerializeField]
        private TMP_InputField staffIdField;

        [SerializeField]
        private TMP_InputField staffJobField;

        [SerializeField]
        private TMP_Dropdown staffJobDropdown;

        [SerializeField]
        private Button deleteItemButton;


        public void InitData(EditorModel editorModel, string staffId, List<string> staffJobs)
        {
            isInit = true;
            this.staffId = staffId;
            this.staffJobs = staffJobs;
            Bind(editorModel);
        }

        public override void Bind(EditorModel editorModel)
        {
            if (!isInit)
            {
                throw new Exception("StaffItem: 未初始化数据");
            }

            base.Bind(editorModel);

            staffIdField.text = staffId;
            staffJobField.text = string.Join("/", staffJobs);
        }
    }
}
