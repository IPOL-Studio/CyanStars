#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class NoteAttributeView : BaseView<NoteAttributeViewModel>
    {
        [Header("图片资源")]
        [SerializeField]
        private Sprite selectedToggleSprite = null!;

        [SerializeField]
        private Sprite unselectedToggleSprite = null!;

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

        [SerializeField]
        private Toggle breakLeftPosToggle = null!;

        [SerializeField]
        private Toggle breakRightPoaToggle = null!;


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
            ViewModel.BreakLeftPosState
                .Subscribe(isOn =>
                    {
                        breakLeftPosToggle.isOn = isOn;
                        breakLeftPosToggle.image.sprite = isOn
                            ? selectedToggleSprite
                            : unselectedToggleSprite;
                    }
                )
                .AddTo(this);
            ViewModel.BreakRightPosState
                .Subscribe(isOn =>
                    {
                        breakRightPoaToggle.isOn = isOn;
                        breakRightPoaToggle.image.sprite = isOn
                            ? selectedToggleSprite
                            : unselectedToggleSprite;
                    }
                )
                .AddTo(this);


            // V -> VM 绑定
            judgeBeatField1.onEndEdit.AddListener(UpdateNoteJudgeBeat);
            judgeBeatField2.onEndEdit.AddListener(UpdateNoteJudgeBeat);
            judgeBeatField3.onEndEdit.AddListener(UpdateNoteJudgeBeat);
            endJudgeBeatField1.onEndEdit.AddListener(UpdateNoteEndJudgeBeat);
            endJudgeBeatField2.onEndEdit.AddListener(UpdateNoteEndJudgeBeat);
            endJudgeBeatField3.onEndEdit.AddListener(UpdateNoteEndJudgeBeat);
            posField.onEndEdit.AddListener(ViewModel.UpdateNotePos);
            breakLeftPosToggle.onValueChanged.AddListener(OnBreakLeftPosToggleChanged);
            breakRightPoaToggle.onValueChanged.AddListener(OnBreakRightPosToggleChanged);
        }

        private void UpdateNoteJudgeBeat(string _) // 确保签名一致以供取消订阅
        {
            ViewModel.UpdateNoteJudgeBeat(
                judgeBeatField1.text,
                judgeBeatField2.text,
                judgeBeatField3.text
            );
        }

        private void UpdateNoteEndJudgeBeat(string _) // 确保签名一致以供取消订阅
        {
            ViewModel.UpdateNoteEndJudgeBeat(
                endJudgeBeatField1.text,
                endJudgeBeatField2.text,
                endJudgeBeatField3.text
            );
        }

        private void OnBreakLeftPosToggleChanged(bool isOn)
        {
            if (!isOn) // Unity Toggle Group 自动取消时
                return;

            ViewModel.UpdateBreakNotePos(BreakNotePos.Left);
            breakLeftPosToggle.image.sprite = selectedToggleSprite;
            breakRightPoaToggle.image.sprite = unselectedToggleSprite;
        }

        private void OnBreakRightPosToggleChanged(bool isOn)
        {
            if (!isOn) // Unity Toggle Group 自动取消时
                return;

            ViewModel.UpdateBreakNotePos(BreakNotePos.Right);
            breakLeftPosToggle.image.sprite = unselectedToggleSprite;
            breakRightPoaToggle.image.sprite = selectedToggleSprite;
        }

        protected override void OnDestroy()
        {
            judgeBeatField1.onEndEdit.RemoveListener(UpdateNoteJudgeBeat);
            judgeBeatField2.onEndEdit.RemoveListener(UpdateNoteJudgeBeat);
            judgeBeatField3.onEndEdit.RemoveListener(UpdateNoteJudgeBeat);
            endJudgeBeatField1.onEndEdit.RemoveListener(UpdateNoteEndJudgeBeat);
            endJudgeBeatField2.onEndEdit.RemoveListener(UpdateNoteEndJudgeBeat);
            endJudgeBeatField3.onEndEdit.RemoveListener(UpdateNoteEndJudgeBeat);
            posField.onEndEdit.RemoveListener(ViewModel.UpdateNotePos);
            breakLeftPosToggle.onValueChanged.RemoveListener(OnBreakLeftPosToggleChanged);
            breakRightPoaToggle.onValueChanged.RemoveListener(OnBreakRightPosToggleChanged);
        }
    }
}
