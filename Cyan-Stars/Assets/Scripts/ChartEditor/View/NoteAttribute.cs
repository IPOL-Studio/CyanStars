using CyanStars.ChartEditor.Model;
using JetBrains.Annotations;
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
        private TMP_InputField endJudgeBeatField1;

        [SerializeField]
        private TMP_InputField endJudgeBeatField2;

        [SerializeField]
        private TMP_InputField endJudgeBeatField3;


        [SerializeField]
        private TMP_InputField posField;

        [SerializeField]
        private Button breakButtonL;

        [SerializeField]
        private Button breakButtonR;


        [SerializeField]
        private Button correctAudioButton;

        [SerializeField]
        private Button hitAudioButton;

        [SerializeField]
        private Button speedGroupButton;

        [SerializeField]
        private TMP_InputField speedOffsetField;

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);


        }
    }
}
