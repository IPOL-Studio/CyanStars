#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CyanStars.Utils.SelectableUI
{
    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(SelectableStateObserver))]
    public class SelectableViewEffectHandler : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField]
        private Transform? transformToChangeScale;

        [SerializeField]
        private Selectable? selectable;

        [SerializeField]
        private SelectableStateObserver? observer;

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


        // 缓存当前的动画序列，用于打断旧动画
        private Sequence? currentEffectSequence;


#if UNITY_EDITOR
        private void Reset()
        {
            if (transformToChangeScale == null)
                transformToChangeScale = GetComponent<Transform>();
            if (selectable == null)
                selectable = GetComponent<Selectable>();
            if (observer == null)
                observer = GetComponent<SelectableStateObserver>();
        }
#endif

        private void Awake()
        {
            if (transformToChangeScale == null)
                transformToChangeScale = GetComponent<Transform>();
            if (selectable == null)
                selectable = GetComponent<Selectable>();
            if (observer == null)
                observer = GetComponent<SelectableStateObserver>();
        }

        private void Start()
        {
            observer!.OnStateChanged.AddListener(OnStateChanged);
        }

        private void OnDestroy()
        {
            observer!.OnStateChanged.RemoveListener(OnStateChanged);

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
            if (transformToChangeScale != null)
            {
                currentEffectSequence.Join(transformToChangeScale
                    .DOScale(config.Scale, tweenDuration)
                    .SetEase(tweenEase));
            }

            // 添加颜色渐变动画
            foreach (var graphic in maskableGraphicToChangeColor)
            {
                if (graphic != null)
                {
                    currentEffectSequence.Join(graphic
                        .DOColor(config.TintColor, tweenDuration)
                        .SetEase(tweenEase));
                }
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
