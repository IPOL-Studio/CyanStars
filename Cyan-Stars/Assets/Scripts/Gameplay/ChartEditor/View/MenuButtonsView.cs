#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
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


        public override void Bind(MenuButtonsViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            functionToggle.isOn = false;
            functionCanvas.enabled = ViewModel.FunctionCanvasVisibility.Value;

            ViewModel.FunctionCanvasVisibility.OnValueChanged += RefreshFunctionCanvas;

            functionToggle.onValueChanged.AddListener(ViewModel.SetFunctionCanvasVisibility);

            chartPackDataButton.onClick.AddListener(() =>
                ViewModel.OpenCanvas(CanvasType.ChartPackDataCanvas));
            chartDataButton.onClick.AddListener(() =>
                ViewModel.OpenCanvas(CanvasType.ChartDataCanvas));
            musicVersionButton.onClick.AddListener(() =>
                ViewModel.OpenCanvas(CanvasType.MusicVersionCanvas));
            bpmGroupButton.onClick.AddListener(() =>
                ViewModel.OpenCanvas(CanvasType.BpmGroupCanvas));
            speedTemplateButton.onClick.AddListener(() =>
                ViewModel.OpenCanvas(CanvasType.SpeedTemplateCanvas));
        }

        private void RefreshFunctionCanvas(bool visible)
        {
            if (functionCanvas.enabled != visible)
            {
                functionCanvas.enabled = visible;
            }
        }

        protected override void OnDestroy()
        {
            ViewModel.FunctionCanvasVisibility.OnValueChanged -= RefreshFunctionCanvas;
            functionToggle.onValueChanged.RemoveAllListeners();
            chartPackDataButton.onClick.RemoveAllListeners();
            chartDataButton.onClick.RemoveAllListeners();
            musicVersionButton.onClick.RemoveAllListeners();
            bpmGroupButton.onClick.RemoveAllListeners();
            speedTemplateButton.onClick.RemoveAllListeners();
        }
    }
}
