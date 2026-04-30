#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 用于控制制谱器弹窗整体的打开/关闭动画效果
    /// </summary>
    /// <remarks>
    /// 挂在到弹窗预制体下，然后由同物体上的 View 脚本调用效果
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public class ChartEditorPopupEffectController : MonoBehaviour
    {
        [SerializeField]
        private float tweenDuration = 0.2f;

        private CanvasGroup canvasGroup = null!;
        private RectTransform rectTransform = null!;
        private Tween? activeSequence;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = (transform as RectTransform)!;
            canvasGroup.alpha = 0;
            rectTransform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }


        /// <summary>
        /// 播放弹窗打开动画
        /// </summary>
        /// <exception cref="OperationCanceledException">
        /// 动画被 CancellationToken 取消
        /// </exception>
        /// <remarks>
        /// 建议在调用前手动启用 Canvas 组件
        /// </remarks>
        public async Task ShowPopupCanvas(CancellationToken token = default)
        {
            activeSequence?.Kill();
            activeSequence = null;

            token.ThrowIfCancellationRequested();

            activeSequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(1, tweenDuration))
                .Join(rectTransform.DOScale(1, tweenDuration))
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject);

            await AwaitTweenSafely(activeSequence, token);
        }

        /// <summary>
        /// 播放弹窗关闭动画
        /// </summary>
        /// <exception cref="OperationCanceledException">
        /// 动画被 CancellationToken 取消
        /// </exception>
        /// <remarks>
        /// 可以通过返回值来在播放完毕后禁用 Canvas 组件
        /// </remarks>
        public async Task HidePopupCanvas(CancellationToken token = default)
        {
            activeSequence?.Kill();
            activeSequence = null;

            token.ThrowIfCancellationRequested();

            activeSequence = DOTween.Sequence()
                .Append(canvasGroup.DOFade(0f, tweenDuration))
                .Join(rectTransform.DOScale(0.8f, tweenDuration))
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .SetLink(gameObject);

            await AwaitTweenSafely(activeSequence, token);
        }


        /// <summary>
        /// 安全等待 Tween 完成
        /// </summary>
        /// <exception cref="OperationCanceledException">
        /// 动画被 CancellationToken 取消
        /// </exception>
        private static async Task AwaitTweenSafely(Tween tween, CancellationToken token)
        {
            try
            {
                await using (token.Register(() => tween.Kill()))
                {
                    await tween.AsyncWaitForCompletion();
                }
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)
                {
                    // 如果是由于 Token 取消导致的任何异常，一律转为 OperationCanceledException
                    throw new OperationCanceledException(token);
                }

                // 否则如果是其他真正的异常，则继续向上抛出
                throw;
            }

            token.ThrowIfCancellationRequested();
        }
    }
}
