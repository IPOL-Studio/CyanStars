#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 制谱器弹窗 View 基类
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class BasePopupView<TViewModel> : BaseView<TViewModel>
        where TViewModel : BaseViewModel
    {
        [SerializeField]
        private Button closeCanvasButton = null!;

        private Canvas canvas = null!;
        private CommandStack commandStack = null!;

        private ChartEditorPopupEffectController? effectController;
        private CancellationTokenSource? cts;
        private readonly ReactiveProperty<bool> CanvasVisibility = new(false);


        protected virtual void Awake()
        {
            canvas = GetComponent<Canvas>();
            TryGetComponent<ChartEditorPopupEffectController>(out effectController);
        }

        protected virtual void OnDestroy()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }


        public override void Bind(TViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            commandStack = GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack;

            CanvasVisibility
                .Subscribe(visible =>
                {
                    if (visible)
                        _ = OpenCanvas();
                    else
                        _ = CloseCanvas();
                })
                .AddTo(this);

            closeCanvasButton
                .OnClickAsObservable()
                .Subscribe(_ => SetCanvasVisibility(false))
                .AddTo(this);
        }

        public void SetCanvasVisibility(bool visible)
        {
            if (CanvasVisibility.CurrentValue == visible)
                return;

            commandStack.ExecuteCommand(
                () => CanvasVisibility.Value = visible,
                () => CanvasVisibility.Value = !visible
            );
        }


        protected virtual async Task OpenCanvas()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            canvas.enabled = true;

            if (effectController != null)
            {
                try
                {
                    await effectController.ShowPopupCanvas(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 被 cts.Token 取消而抛出异常，Pass
                }
            }
        }

        protected virtual async Task CloseCanvas()
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();

            if (effectController != null)
            {
                try
                {
                    await effectController.HidePopupCanvas(cts.Token);
                    canvas.enabled = false;
                }
                catch (OperationCanceledException)
                {
                    // 被 cts.Token 取消而抛出异常，Pass
                }
            }
            else
            {
                canvas.enabled = false;
            }
        }
    }
}
