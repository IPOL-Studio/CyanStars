#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CyanStars.Utils.SelectableUI
{
    /// <summary>
    /// 用于为 Button 和 Toggle 处理玩家交互视觉效果
    /// </summary>
    /// <remarks>
    /// 用法：
    /// 1. 把此脚本和 SelectableStateObserver 挂载在 Button 或 toggle 物体上
    /// 2. 把所有需要在交互时变色的图片/文字拖到 maskableGraphicToChangeColor 里面
    /// 3. 调整效果直到满意
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(SelectableStateObserver))]
    public class SelectableViewEffectHandler : MonoBehaviour
    {
        [Header("动画设置")]
        [SerializeField]
        private float tweenDuration = 0.1f;

        [SerializeField]
        private Ease tweenEase = Ease.OutQuad;

        [SerializeField]
        private List<RectTransform> rectTransformToChangeScale = new();

        [SerializeField]
        private List<Graphic> graphicToChangeColor = new();

        [Header("各状态配置")]
        [SerializeField]
        private StateEffectConfig normal = new() { Scale = Vector3.one, TintColor = new Color(0.93f, 0.93f, 0.93f, 0.98f) };

        [SerializeField]
        private StateEffectConfig hover = new() { Scale = new Vector3(1.02f, 1.02f, 1.02f), TintColor = new Color(0.98f, 0.98f, 0.98f, 0.98f) };

        [SerializeField]
        private StateEffectConfig pressed = new() { Scale = new Vector3(0.98f, 0.98f, 0.98f), TintColor = new Color(0.62f, 0.62f, 0.62f, 0.98f) };

        [SerializeField]
        private StateEffectConfig disabled = new() { Scale = Vector3.one, TintColor = new Color(0.33f, 0.33f, 0.33f, 0.98f) };

        [SerializeField]
        private StateEffectConfig normalSelected = new() { Scale = Vector3.one, TintColor = new Color(0.27f, 0.47f, 0.82f, 0.98f) };

        [SerializeField]
        private StateEffectConfig hoverSelected = new() { Scale = new Vector3(1.02f, 1.02f, 1.02f), TintColor = new Color(0.49f, 0.63f, 0.87f, 0.98f) };

        [SerializeField]
        private StateEffectConfig pressedSelected = new() { Scale = new Vector3(0.98f, 0.98f, 0.98f), TintColor = new Color(0.29f, 0.42f, 0.62f, 0.98f) };

        [SerializeField]
        private StateEffectConfig disabledSelected = new() { Scale = Vector3.one, TintColor = new Color(0.11f, 0.19f, 0.33f, 0.98f) };


        private Selectable selectable = null!;
        private SelectableStateObserver observer = null!;
        private Sequence? currentEffectSequence; // 缓存当前的动画序列，用于打断旧动画


        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            observer = GetComponent<SelectableStateObserver>();
        }

        private void OnEnable()
        {
            OnStateChanged(observer.CurrentState, true);
            observer.OnStateChanged.AddListener(OnStateChangedListener);
        }

        private void OnDisable()
        {
            observer.OnStateChanged.RemoveListener(OnStateChangedListener);
            currentEffectSequence?.Kill();
            currentEffectSequence = null;
        }

        private void OnStateChangedListener(UIState state) => OnStateChanged(state, false);

        private void OnStateChanged(UIState state, bool instant)
        {
            var targetConfig = state switch
            {
                UIState.Normal => normal,
                UIState.NormalSelected => normalSelected,
                UIState.Hover => hover,
                UIState.HoverSelected => hoverSelected,
                UIState.Pressed => pressed,
                UIState.PressedSelected => pressedSelected,
                UIState.Disabled => disabled,
                UIState.DisabledSelected => disabledSelected,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

            ApplyEffect(targetConfig, instant);
        }

        /// <summary>
        /// 应用动画效果
        /// </summary>
        private void ApplyEffect(StateEffectConfig config, bool instant)
        {
            currentEffectSequence?.Kill();

            if (instant)
            {
                foreach (var rectTransform in rectTransformToChangeScale)
                {
                    rectTransform.localScale = config.Scale;
                }

                foreach (var graphic in graphicToChangeColor)
                {
                    if (graphic != null) graphic.color = config.TintColor;
                }

                return;
            }

            currentEffectSequence = DOTween.Sequence();

            // 添加缩放动画
            foreach (var rectTransform in rectTransformToChangeScale)
            {
                if (rectTransform != null)
                {
                    currentEffectSequence.Join(rectTransform
                        .DOScale(config.Scale, tweenDuration)
                        .SetEase(tweenEase)
                    );
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError($"rectTransform 为 null，无法更新视觉效果，请检查", gameObject);
                }
#endif
            }

            // 添加颜色渐变动画
            foreach (var graphic in graphicToChangeColor)
            {
                if (graphic != null)
                {
                    currentEffectSequence.Join(graphic
                        .DOColor(config.TintColor, tweenDuration)
                        .SetEase(tweenEase)
                    );
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogError($"graphic 为 null，无法更新视觉效果，请检查", gameObject);
                }
#endif
            }

            currentEffectSequence.SetLink(gameObject);
        }
    }

    [Serializable]
    public class StateEffectConfig
    {
        public Vector3 Scale = Vector3.one;
        public Color TintColor = Color.white;
    }
}
