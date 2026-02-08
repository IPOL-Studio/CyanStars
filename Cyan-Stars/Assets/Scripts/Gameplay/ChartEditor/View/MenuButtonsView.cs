#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// Menu 下拉菜单中每个按钮持有的 V。
    /// </summary>
    public class MenuButtonsView : BaseView<MenuButtonsViewModel>
    {
        [SerializeField]
        private Toggle functionToggle = null!;

        [SerializeField]
        private Button saveButton = null!;

        [SerializeField]
        private Button testButton = null!; // TODO

        [SerializeField]
        private Button undoButton = null!;

        [SerializeField]
        private Button redoButton = null!;


        [Header("功能按钮相关")]
        [SerializeField]
        private Canvas functionCanvas = null!;

        [SerializeField]
        private Button chartPackDataButton = null!;

        [SerializeField]
        private Button chartDataButton = null!;

        [SerializeField]
        private Button musicVersionButton = null!;

        [SerializeField]
        private Button bpmGroupButton = null!;

        [SerializeField]
        private Button speedTemplateButton = null!; // TODO

        [SerializeField]
        private Button exitSimplificationModeButton = null!;

        [SerializeField]
        private Button enterSimplificationModeButton = null!;

        [Header("CanvasView 绑定")]
        [SerializeField]
        private ChartPackDataView chartPackDataView = null!;

        [SerializeField]
        private ChartDataView chartDataView = null!;

        [SerializeField]
        private MusicVersionView musicVersionView = null!;

        [SerializeField]
        private BpmGroupView bpmGroupView = null!;


        private readonly ReactiveProperty<bool> FunctionCanvasVisibility = new ReactiveProperty<bool>(false);


        public override void Bind(MenuButtonsViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            FunctionCanvasVisibility
                .Subscribe(isVisible =>
                    {
                        functionToggle.SetIsOnWithoutNotify(isVisible);
                        functionCanvas.enabled = isVisible;
                    }
                )
                .AddTo(this);
            ViewModel.IsSimplificationMode
                .Subscribe(isSimplificationMode =>
                {
                    // speedTemplateButton.gameObject.SetActive(!isSimplificationMode);
                    enterSimplificationModeButton.gameObject.SetActive(!isSimplificationMode);
                    exitSimplificationModeButton.gameObject.SetActive(isSimplificationMode);
                })
                .AddTo(this);


            functionToggle.onValueChanged.AddListener(SetFunctionCanvasVisibility);
            saveButton.onClick.AddListener(ViewModel.SaveFileToDesk);
            // testButton.onClick.AddListener();
            undoButton.onClick.AddListener(ViewModel.Undo);
            redoButton.onClick.AddListener(ViewModel.Redo);

            chartPackDataButton.onClick.AddListener(chartPackDataView.OpenCanvas);
            chartDataButton.onClick.AddListener(chartDataView.OpenCanvas);
            musicVersionButton.onClick.AddListener(musicVersionView.OpenCanvas);
            bpmGroupButton.onClick.AddListener(bpmGroupView.OpenCanvas);
            // speedTemplateButton.onClick.AddListener();

            exitSimplificationModeButton.onClick.AddListener(() => ViewModel.SetSimplificationMode(false));
            enterSimplificationModeButton.onClick.AddListener(() => ViewModel.SetSimplificationMode(true));
        }

        private void SetFunctionCanvasVisibility(bool visibility)
        {
            FunctionCanvasVisibility.Value = visibility;
        }

        protected override void OnDestroy()
        {
            functionToggle.onValueChanged.RemoveListener(SetFunctionCanvasVisibility);
            saveButton.onClick.RemoveListener(ViewModel.SaveFileToDesk);
            // testButton.onClick.RemoveAllListeners();
            undoButton.onClick.RemoveListener(ViewModel.Undo);
            redoButton.onClick.RemoveListener(ViewModel.Redo);
            chartPackDataButton.onClick.RemoveListener(chartPackDataView.OpenCanvas);
            chartDataButton.onClick.RemoveListener(chartDataView.OpenCanvas);
            musicVersionButton.onClick.RemoveListener(musicVersionView.OpenCanvas);
            bpmGroupButton.onClick.RemoveListener(bpmGroupView.OpenCanvas);
            // speedTemplateButton.onClick.RemoveAllListeners();
            exitSimplificationModeButton.onClick.RemoveAllListeners();
            enterSimplificationModeButton.onClick.RemoveAllListeners();
        }
    }
}
