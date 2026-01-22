#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class NoteAttributeView : BaseView<NoteAttributeViewModel>
    {
        [Header("Frames")]
        [SerializeField]
        private GameObject noteAttributeFrame = null!;

        [SerializeField]
        private GameObject judgeBeatFrame = null!;

        [SerializeField]
        private GameObject endJudgeBeatFrame = null!;

        [SerializeField]
        private GameObject posFrame = null!;

        [SerializeField]
        private GameObject breakPosFrame = null!;

        [SerializeField]
        private GameObject correctAudioFrame = null!;

        [SerializeField]
        private GameObject hitAudioFrame = null!;

        [SerializeField]
        private GameObject speedTemplateFrame = null!;

        [SerializeField]
        private GameObject speedOffsetFrame = null!;

        [Header("Sub Objects")]
        [SerializeField]
        private TMP_InputField judgeBeatField1 = null!;

        [SerializeField]
        private TMP_InputField judgeBeatField2 = null!;

        [SerializeField]
        private TMP_InputField judgeBeatField3 = null!;

        [SerializeField]
        private TMP_InputField endJudgeBeatField1 = null!;

        [SerializeField]
        private TMP_InputField endJudgeBeatField2 = null!;

        [SerializeField]
        private TMP_InputField endJudgeBeatField3 = null!;

        [SerializeField]
        private TMP_InputField posField3 = null!;

        public override void Bind(NoteAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
        }

        protected override void OnDestroy()
        {
        }
    }
}
