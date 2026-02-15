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


            functionToggle
                .OnValueChangedAsObservable()
                .Subscribe(SetFunctionCanvasVisibility)
                .AddTo(this);
            functionToggle
                .OnValueChangedAsObservable()
                .Subscribe(SetFunctionCanvasVisibility)
                .AddTo(this);
            saveButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.SaveFileToDesk())
                .AddTo(this);
            // testButton ...
            undoButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.Undo())
                .AddTo(this);
            redoButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.Redo())
                .AddTo(this);

            chartPackDataButton
                .OnClickAsObservable()
                .Subscribe(_ => chartPackDataView.OpenCanvas())
                .AddTo(this);
            chartDataButton
                .OnClickAsObservable()
                .Subscribe(_ => chartDataView.OpenCanvas())
                .AddTo(this);
            musicVersionButton
                .OnClickAsObservable()
                .Subscribe(_ => musicVersionView.OpenCanvas())
                .AddTo(this);
            bpmGroupButton
                .OnClickAsObservable()
                .Subscribe(_ => bpmGroupView.OpenCanvas())
                .AddTo(this);
            // speedTemplateButton ...

            exitSimplificationModeButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.SetSimplificationMode(false))
                .AddTo(this);
            enterSimplificationModeButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.SetSimplificationMode(true))
                .AddTo(this);
        }

        private void SetFunctionCanvasVisibility(bool visibility)
        {
            FunctionCanvasVisibility.Value = visibility;
        }
    }
}
