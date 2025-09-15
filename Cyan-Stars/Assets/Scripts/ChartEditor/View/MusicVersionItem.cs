using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class MusicVersionItem : BaseView
    {
        private bool isInit = false;
        private MusicVersionData musicVersionData;


        [SerializeField]
        private TMP_InputField titleField;

        [SerializeField]
        private TMP_Text audioFilePath;

        [SerializeField]
        private Button importMusicButton;

        [SerializeField]
        private TMP_InputField offsetField;

        [SerializeField]
        private Button subOffsetButton;

        [SerializeField]
        private Button addOffsetButton;

        [SerializeField]
        private Button testOffsetButton;

        [SerializeField]
        private TMP_InputField previewStartBeatField1;

        [SerializeField]
        private TMP_InputField previewStartBeatField2;

        [SerializeField]
        private TMP_InputField previewStartBeatField3;

        [SerializeField]
        private TMP_InputField previewEndBeatField1;

        [SerializeField]
        private TMP_InputField previewEndBeatField2;

        [SerializeField]
        private TMP_InputField previewEndBeatField3;

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
        private Button setDefaultMusicVersionItemButton;


        public void InitData(EditorModel editorModel, MusicVersionData musicVersionData)
        {
            isInit = true;
            this.musicVersionData = musicVersionData;
            Bind(editorModel);
        }

        public override void Bind(EditorModel editorModel)
        {
            if (!isInit)
            {
                throw new Exception("MusicVersionItem: 未初始化数据");
            }

            base.Bind(editorModel);

            titleField.text = musicVersionData.VersionTitle;
            audioFilePath.text = musicVersionData.AudioFilePath;
            offsetField.text = musicVersionData.Offset.ToString();
            previewStartBeatField1.text = musicVersionData.PreviewStartBeat.IntegerPart.ToString();
            previewStartBeatField2.text = musicVersionData.PreviewStartBeat.Numerator.ToString();
            previewStartBeatField3.text = musicVersionData.PreviewStartBeat.Denominator.ToString();
            previewEndBeatField1.text = musicVersionData.PreviewEndBeat.IntegerPart.ToString();
            previewEndBeatField2.text = musicVersionData.PreviewEndBeat.Numerator.ToString();
            previewEndBeatField3.text = musicVersionData.PreviewEndBeat.Denominator.ToString();
            foreach (KeyValuePair<string, List<string>> item in musicVersionData.Staffs)
            {
                GameObject go = Instantiate(staffItemPrefab, staffInfoFrameObject.transform);
                go.transform.SetSiblingIndex(staffInfoFrameObject.transform.childCount - 3);
                go.GetComponent<StaffItem>().InitData(Model, item.Key, item.Value);
            }
        }
    }
}
