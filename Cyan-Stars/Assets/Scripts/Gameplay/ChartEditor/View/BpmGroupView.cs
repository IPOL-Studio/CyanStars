#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class BpmGroupView : BaseView<BpmGroupViewModel>
    {
        [Header("列表子 View")]
        [SerializeField]
        private GameObject listItemPrefab = null!;

        [SerializeField]
        private GameObject bpmListGameObject = null!;


        [Header("主 View")]
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

        [SerializeField]
        private GameObject timelineGameObject = null!;

        [SerializeField]
        private GameObject detailFrameGameObject = null!;

        [SerializeField]
        private TMP_Text detailCountText = null!;

        [SerializeField]
        private TMP_InputField bpmInputField = null!;

        [SerializeField]
        private Button testBpmButton = null!;

        [SerializeField]
        private TMP_InputField startBeatField1 = null!;

        [SerializeField]
        private TMP_InputField startBeatField2 = null!;

        [SerializeField]
        private TMP_InputField startBeatField3 = null!;

        [SerializeField]
        private Button deleteItemButton = null!;

        [SerializeField]
        private Toggle audioToggle = null!;

        [SerializeField]
        private Button testButton = null!;


        public override void Bind(BpmGroupViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.CanvasVisible
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            ViewModel.ListVisible
                .Subscribe(visible => bpmListGameObject.SetActive(visible))
                .AddTo(this);
            ViewModel.SelectedBpmItem
                .Subscribe(selectedItem => detailFrameGameObject.SetActive(selectedItem != null))
                .AddTo(this);

            ViewModel.BPMText
                .Subscribe(text => bpmInputField.text = text)
                .AddTo(this);
            ViewModel.StartBeatText1
                .Subscribe(text => startBeatField1.text = text)
                .AddTo(this);
            ViewModel.StartBeatText2
                .Subscribe(text => startBeatField2.text = text)
                .AddTo(this);
            ViewModel.StartBeatText3
                .Subscribe(text => startBeatField3.text = text)
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
        }
    }
}
