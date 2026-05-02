#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using DG.Tweening;
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
        private CanvasGroup functionCanvasGroup = null!;

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

        [SerializeField]
        private Button exitChartEditorButton = null!;

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
        private readonly List<ShortcutCommand.ListenerDisposable> ShortcutListeners = new();

        private Canvas functionCanvas = null!;


        private void OnEnable()
        {
            ShortcutListeners.Add(ShortcutCommandRegistry.Save.RegisterListener(OnSaveRequested));
            ShortcutListeners.Add(ShortcutCommandRegistry.Undo.RegisterListener(OnUndoRequested));
            ShortcutListeners.Add(ShortcutCommandRegistry.Redo.RegisterListener(OnRedoRequested));
        }

        public override void Bind(MenuButtonsViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            functionCanvas = functionCanvasGroup.GetComponent<Canvas>();

            FunctionCanvasVisibility
                .Subscribe(isVisible =>
                    {
                        functionToggle.SetIsOnWithoutNotify(isVisible);

                        if (isVisible)
                        {
                            functionCanvas.enabled = true;
                            functionCanvasGroup
                                .DOFade(1f, 0.05f)
                                .SetEase(Ease.OutQuad);
                            ((RectTransform)functionCanvas.gameObject.transform)
                                .DOScale(1f, 0.05f)
                                .SetEase(Ease.OutQuad);
                        }
                        else
                        {
                            ((RectTransform)functionCanvas.gameObject.transform)
                                .DOScale(0.9f, 0.05f)
                                .SetEase(Ease.OutQuad);
                            functionCanvasGroup
                                .DOFade(0f, 0.05f)
                                .SetEase(Ease.OutQuad)
                                .OnComplete(() => functionCanvas.enabled = false);
                        }
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
                .Subscribe(_ => OnSaveRequested())
                .AddTo(this);
            // testButton ...
            undoButton
                .OnClickAsObservable()
                .Subscribe(_ => OnUndoRequested())
                .AddTo(this);
            redoButton
                .OnClickAsObservable()
                .Subscribe(_ => OnRedoRequested())
                .AddTo(this);

            chartPackDataButton
                .OnClickAsObservable()
                .Subscribe(_ => chartPackDataView.SetCanvasVisibility(true))
                .AddTo(this);
            chartDataButton
                .OnClickAsObservable()
                .Subscribe(_ => chartDataView.SetCanvasVisibility(true))
                .AddTo(this);
            musicVersionButton
                .OnClickAsObservable()
                .Subscribe(_ => musicVersionView.SetCanvasVisibility(true))
                .AddTo(this);
            bpmGroupButton
                .OnClickAsObservable()
                .Subscribe(_ => bpmGroupView.SetCanvasVisibility(true))
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
            exitChartEditorButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ExitChartEditor())
                .AddTo(this);
        }

        private void SetFunctionCanvasVisibility(bool visibility)
        {
            FunctionCanvasVisibility.Value = visibility;
        }

        private void OnSaveRequested() => ViewModel.SaveFileToDesk();
        private void OnUndoRequested() => ViewModel.Undo();
        private void OnRedoRequested() => ViewModel.Redo();

        private void OnDisable()
        {
            for (int i = ShortcutListeners.Count - 1; i >= 0; i--)
            {
                ShortcutListeners[i].Dispose();
            }
        }
    }
}
