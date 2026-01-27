#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
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
        private Button saveButton = null!; // TODO

        [SerializeField]
        private Button testButton = null!; // TODO

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


        public override void Bind(MenuButtonsViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.FunctionCanvasVisibility
                .Subscribe(isVisible =>
                {
                    functionToggle.SetIsOnWithoutNotify(isVisible);
                    functionCanvas.enabled = isVisible;
                })
                .AddTo(this);
            ViewModel.IsSimplificationMode
                .Subscribe(isSimplificationMode =>
                {
                    speedTemplateButton.gameObject.SetActive(!isSimplificationMode);
                    enterSimplificationModeButton.gameObject.SetActive(!isSimplificationMode);
                    exitSimplificationModeButton.gameObject.SetActive(isSimplificationMode);
                })
                .AddTo(this);


            functionToggle.onValueChanged.AddListener(ViewModel.SetFunctionCanvasVisibility);
            saveButton.onClick.AddListener(ViewModel.SaveFileToDesk);

            chartPackDataButton.onClick.AddListener(chartPackDataView.OpenCanvas);
            chartDataButton.onClick.AddListener(chartDataView.OpenCanvas);
            musicVersionButton.onClick.AddListener(musicVersionView.OpenCanvas);
            bpmGroupButton.onClick.AddListener(bpmGroupView.OpenCanvas);
            // speedTemplateButton.onClick.AddListener();

            exitSimplificationModeButton.onClick.AddListener(() => ViewModel.SetSimplificationMode(false));
            enterSimplificationModeButton.onClick.AddListener(() => ViewModel.SetSimplificationMode(true));
        }

        protected override void OnDestroy()
        {
            functionToggle.onValueChanged.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            chartPackDataButton.onClick.RemoveAllListeners();
            chartDataButton.onClick.RemoveAllListeners();
            musicVersionButton.onClick.RemoveAllListeners();
            bpmGroupButton.onClick.RemoveAllListeners();
            speedTemplateButton.onClick.RemoveAllListeners();
            exitSimplificationModeButton.onClick.RemoveAllListeners();
            enterSimplificationModeButton.onClick.RemoveAllListeners();
        }
    }
}
