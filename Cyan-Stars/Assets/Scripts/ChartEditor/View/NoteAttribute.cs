using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class NoteAttribute : BaseView
    {
        [SerializeField]
        private TMP_InputField judgeBeatField1;

        [SerializeField]
        private TMP_InputField judgeBeatField2;

        [SerializeField]
        private TMP_InputField judgeBeatField3;


        [SerializeField]
        private TMP_InputField endBeatField1;

        [SerializeField]
        private TMP_InputField endBeatField2;

        [SerializeField]
        private TMP_InputField endBeatField3;


        [SerializeField]
        private TMP_InputField posField;

        [SerializeField]
        private Button breakButtonL;

        [SerializeField]
        private Button breakButtonR;


        [SerializeField]
        private Button correctAudioButton; // TODO

        [SerializeField]
        private Button hitAudioButton; // TODO

        [SerializeField]
        private Button speedGroupButton; // TODO

        [SerializeField]
        private TMP_InputField speedOffsetField; // TODO


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            judgeBeatField1.onEndEdit.AddListener((text) => Model.SetJudgeBeat(text, null, null));
            judgeBeatField2.onEndEdit.AddListener((text) => Model.SetJudgeBeat(null, text, null));
            judgeBeatField3.onEndEdit.AddListener((text) => Model.SetJudgeBeat(null, null, text));
            endBeatField1.onEndEdit.AddListener((text) => Model.SetEndBeat(text, null, null));
            endBeatField2.onEndEdit.AddListener((text) => Model.SetEndBeat(null, text, null));
            endBeatField3.onEndEdit.AddListener((text) => Model.SetEndBeat(null, null, text));
            posField.onEndEdit.AddListener((text) => Model.SetPos(text));
            breakButtonL.onClick.AddListener(() => { Model.SetBreakPos(BreakNotePos.Left); });
            breakButtonR.onClick.AddListener(() => { Model.SetBreakPos(BreakNotePos.Right); });

            Model.OnSelectedNoteIDsChanged += RefreshNoteAttribute;
            Model.OnNoteAttributeChanged += RefreshNoteAttribute;
        }

        /// <summary>
        /// 当选择的音符变化时，或音符的属性变化时，刷新属性面板
        /// </summary>
        public void RefreshNoteAttribute()
        {
            // TODO
        }

        private void OnDestroy()
        {
            Model.OnSelectedNoteIDsChanged -= RefreshNoteAttribute;
            Model.OnNoteAttributeChanged -= RefreshNoteAttribute;
        }
    }
}
