#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
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
        private TMP_InputField posField = null!;


        public override void Bind(NoteAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            // Frame 绑定
            ViewModel.FrameVisibility
                .Subscribe(visibility => noteAttributeFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.JudgeBeatFrameVisibility
                .Subscribe(visibility => judgeBeatFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.EndJudgeBeatFrameVisibility
                .Subscribe(visibility => endJudgeBeatFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.PosFrameVisibility
                .Subscribe(visibility => posFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.BreakPosFrameVisibility
                .Subscribe(visibility => breakPosFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.CorrectAudioFrameVisibility
                .Subscribe(visibility => correctAudioFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.HitAudioFrameVisibility
                .Subscribe(visibility => hitAudioFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.SpeedTemplateFrameVisibility
                .Subscribe(visibility => speedTemplateFrame.SetActive(visibility))
                .AddTo(this);
            ViewModel.SpeedOffsetFrameVisibility
                .Subscribe(visibility => speedOffsetFrame.SetActive(visibility))
                .AddTo(this);

            // 属性绑定
            ViewModel.JudgeBeatField1Text
                .Subscribe(text => judgeBeatField1.text = text)
                .AddTo(this);
            ViewModel.JudgeBeatField2Text
                .Subscribe(text => judgeBeatField2.text = text)
                .AddTo(this);
            ViewModel.JudgeBeatField3Text
                .Subscribe(text => judgeBeatField3.text = text)
                .AddTo(this);

            ViewModel.EndJudgeBeatField1Text
                .Subscribe(text => endJudgeBeatField1.text = text)
                .AddTo(this);
            ViewModel.EndJudgeBeatField2Text
                .Subscribe(text => endJudgeBeatField2.text = text)
                .AddTo(this);
            ViewModel.EndJudgeBeatField3Text
                .Subscribe(text => endJudgeBeatField3.text = text)
                .AddTo(this);

            ViewModel.PosFieldText
                .Subscribe(text => posField.text = text)
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
        }
    }
}
