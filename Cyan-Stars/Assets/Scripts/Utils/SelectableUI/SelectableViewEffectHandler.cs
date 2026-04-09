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
        [SerializeField]
        private List<MaskableGraphic> maskableGraphicToChangeColor = new();

        [Header("动画设置")]
        [SerializeField]
        private float tweenDuration = 0.15f;

        [SerializeField]
        private Ease tweenEase = Ease.OutQuad;

        [Header("各状态配置")]
        [SerializeField]
        private StateEffectConfig normalState = new() { Scale = Vector3.one, TintColor = new Color(1f, 1f, 1f, 0.98f) };

        [SerializeField]
        private StateEffectConfig hoverState = new() { Scale = new Vector3(1.01f, 1.01f, 1.01f), TintColor = new Color(1f, 0.86f, 0.4f, 0.98f) };

        [SerializeField]
        private StateEffectConfig pressedState = new() { Scale = new Vector3(0.99f, 0.99f, 0.99f), TintColor = new Color(0.44f, 0.37f, 0.16f, 0.98f) };

        [SerializeField]
        private StateEffectConfig selectedState = new() { Scale = Vector3.one, TintColor = new Color(0.81f, 0.64f, 0.06f, 0.98f) };

        [SerializeField]
        private StateEffectConfig disabledState = new() { Scale = Vector3.one, TintColor = new Color(0.7f, 0.7f, 0.7f, 0.95f) };

        private Transform transformToChangeScale = null!;
        private Selectable selectable = null!;
        private SelectableStateObserver observer = null!;
        private Sequence? currentEffectSequence; // 缓存当前的动画序列，用于打断旧动画


        private void Awake()
        {
            transformToChangeScale = GetComponent<Transform>();
            selectable = GetComponent<Selectable>();
            observer = GetComponent<SelectableStateObserver>();
        }

        private void Start()
        {
            OnStateChanged(observer.CurrentState); // 启动时更新一次视觉效果
            observer.OnStateChanged.AddListener(OnStateChanged);
        }

        private void OnDestroy()
        {
            observer.OnStateChanged.RemoveListener(OnStateChanged);

            currentEffectSequence?.Kill();
            currentEffectSequence = null;
        }


        private void OnStateChanged(UIState state)
        {
            switch (state)
            {
                case UIState.Normal:
                    ApplyEffect(normalState);
                    break;
                case UIState.Hover:
                    ApplyEffect(hoverState);
                    break;
                case UIState.Selected:
                    ApplyEffect(selectedState);
                    break;
                case UIState.Pressed:
                    ApplyEffect(pressedState);
                    break;
                case UIState.Disabled:
                    ApplyEffect(disabledState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        /// <summary>
        /// 应用动画效果
        /// </summary>
        private void ApplyEffect(StateEffectConfig config)
        {
            currentEffectSequence?.Kill();
            currentEffectSequence = DOTween.Sequence();

            // 添加缩放动画
            currentEffectSequence.Join(transformToChangeScale
                .DOScale(config.Scale, tweenDuration)
                .SetEase(tweenEase));


            // 添加颜色渐变动画
            foreach (var graphic in maskableGraphicToChangeColor)
            {
                if (graphic != null)
                {
                    currentEffectSequence.Join(graphic
                        .DOColor(config.TintColor, tweenDuration)
                        .SetEase(tweenEase));
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
