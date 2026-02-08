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
        private ToggleGroup breakPosToggleGroup = null!;

        [SerializeField]
        private Toggle breakLeftPosToggle = null!;

        [SerializeField]
        private Toggle breakRightPosToggle = null!;


        private ReadOnlyReactiveProperty<bool> frameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> judgeBeatFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> endJudgeBeatFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> posFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> breakPosFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> correctAudioFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> hitAudioFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> speedTemplateFrameVisibility = null!;
        private ReadOnlyReactiveProperty<bool> speedOffsetFrameVisibility = null!;


        public override void Bind(NoteAttributeViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            frameVisibility = ViewModel.SelectedNoteData
                .Select(note => note != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(this);
            judgeBeatFrameVisibility = new ReactiveProperty<bool>(true);
            endJudgeBeatFrameVisibility = ViewModel.SelectedNoteData
                .Select(note => note?.Type == NoteType.Hold)
                .ToReadOnlyReactiveProperty()
                .AddTo(this);
            posFrameVisibility = ViewModel.SelectedNoteData
                .Select(note => note?.Type != NoteType.Break)
                .ToReadOnlyReactiveProperty();
            breakPosFrameVisibility = ViewModel.SelectedNoteData
                .Select(note => note?.Type == NoteType.Break)
                .ToReadOnlyReactiveProperty();
            correctAudioFrameVisibility = new ReactiveProperty<bool>(false); // TODO
            hitAudioFrameVisibility = new ReactiveProperty<bool>(false); // TODO
            speedTemplateFrameVisibility = new ReactiveProperty<bool>(false); // TODO
            speedOffsetFrameVisibility = new ReactiveProperty<bool>(false); // TODO

            // Frame 绑定
            frameVisibility
                .Subscribe(visibility => noteAttributeFrame.SetActive(visibility))
                .AddTo(this);
            judgeBeatFrameVisibility
                .Subscribe(visibility => judgeBeatFrame.SetActive(visibility))
                .AddTo(this);
            endJudgeBeatFrameVisibility
                .Subscribe(visibility => endJudgeBeatFrame.SetActive(visibility))
                .AddTo(this);
            posFrameVisibility
                .Subscribe(visibility => posFrame.SetActive(visibility))
                .AddTo(this);
            breakPosFrameVisibility
                .Subscribe(visibility => breakPosFrame.SetActive(visibility))
                .AddTo(this);
            correctAudioFrameVisibility
                .Subscribe(visibility => correctAudioFrame.SetActive(visibility))
                .AddTo(this);
            hitAudioFrameVisibility
                .Subscribe(visibility => hitAudioFrame.SetActive(visibility))
                .AddTo(this);
            speedTemplateFrameVisibility
                .Subscribe(visibility => speedTemplateFrame.SetActive(visibility))
                .AddTo(this);
            speedOffsetFrameVisibility
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
            ViewModel.BreakNotePos
                .Subscribe(breakPos =>
                    {
                        if (breakPos == null)
                        {
                            breakPosToggleGroup.SetAllTogglesOff();
                            return;
                        }

                        breakLeftPosToggle.isOn = breakPos == BreakNotePos.Left;
                        breakRightPosToggle.isOn = breakPos == BreakNotePos.Right;
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
            breakRightPosToggle.onValueChanged.AddListener(OnBreakRightPosToggleChanged);
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
            breakPosToggleGroup.allowSwitchOff = false; // 在首次初始化时禁止 ToggleGroup 自动选一个 Toggle
            breakLeftPosToggle.image.sprite = selectedToggleSprite;
            breakRightPosToggle.image.sprite = unselectedToggleSprite;
        }

        private void OnBreakRightPosToggleChanged(bool isOn)
        {
            if (!isOn) // Unity Toggle Group 自动取消时
                return;

            ViewModel.UpdateBreakNotePos(BreakNotePos.Right);
            breakPosToggleGroup.allowSwitchOff = false; // 在首次初始化时禁止 ToggleGroup 自动选一个 Toggle
            breakLeftPosToggle.image.sprite = unselectedToggleSprite;
            breakRightPosToggle.image.sprite = selectedToggleSprite;
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
            breakRightPosToggle.onValueChanged.RemoveListener(OnBreakRightPosToggleChanged);
        }
    }
}
