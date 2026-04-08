#nullable enable

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils.SelectableUI
{
    /// <summary>
    /// 用于为 Button 和 Toggle 提供玩家 UI 交互状态触发
    /// </summary>
    /// <remarks>
    /// 玩家在 UI 上点击/悬浮/导航时，本脚本会检测交互、更新状态、触发回调，但不直接负责渲染动效
    /// 注意不要再直接修改 selectable.interactable，改为调用 SetInteractable()
    /// 用法：
    /// 1. 挂载在 Button 或 toggle 物体上
    /// 2. 将 Button 或 toggle 组件的过渡设为 None，否则程序会在运行时警告并覆盖
    /// 3. 按照正常的 Unity 方式（onClick/onValueChanged.AddListener 或拖拽）监听并处理 Selectable 的逻辑事件
    /// 4. 再写一个脚本监听并处理本脚本的 OnStateChanged 视图事件
    /// </remarks>
    [RequireComponent(typeof(Selectable))]
    public class SelectableStateObserver : UIBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        public UIState CurrentState { get; private set; } = UIState.Normal;


        [Header("状态改变回调")]
        public UIStateChangeEvent OnStateChanged = new();

        [SerializeField]
        private Selectable? selectable;


        /// <remarks>
        /// selectable as Toggle
        /// </remarks>
        private Toggle? toggle;


        private bool isHovered = false; // 鼠标悬浮
        private bool isFocused = false; // 键盘/手柄导航焦点
        private bool isPressed = false; // 按住


        /// <summary>
        /// 外部手动修改组件可交互性，同时一并触发视觉效果更新
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            // 考虑到 UI 内可能有大量组件，故不采用 Update() 自动轮询
            selectable!.interactable = interactable;
            EvaluateState();
        }


#if UNITY_EDITOR
        protected override void Reset()
        {
            // Unity 编辑器内自动抓取组件，无需手动拖拽
            base.Reset();
            if (selectable == null)
                selectable = GetComponent<Selectable>();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            if (selectable == null)
                selectable = GetComponent<Selectable>();

            if (selectable.transition != Selectable.Transition.None)
            {
                Debug.LogWarning($"{nameof(SelectableStateObserver)} 已自动禁用 selectable.transition，请检查是否为预期效果。", gameObject);
                selectable.transition = Selectable.Transition.None;
            }

            toggle = selectable as Toggle;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (toggle != null)
                toggle.onValueChanged.AddListener(OnToggleValueChanged);


            isHovered = false;
            isFocused = false;
            isPressed = false;
            EvaluateState();
        }

        protected override void OnCanvasGroupChanged()
        {
            base.OnCanvasGroupChanged();
            EvaluateState();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);


            isHovered = false;
            isFocused = false;
            isPressed = false;
            CurrentState = UIState.Normal;
        }


        /// <summary>
        /// 当 selectable 是 toggle 时，外界改变 toggle.isOn 时也会自动改变视觉效果
        /// </summary>
        private void OnToggleValueChanged(bool value)
        {
            EvaluateState();
        }

        private void EvaluateState()
        {
            UIState newState;

            if (!selectable!.IsInteractable())
            {
                // 禁止交互
                newState = UIState.Disabled;
            }
            else if (isPressed && (isHovered || isFocused))
            {
                // 按下，且鼠标不移出组件区域/是键盘手柄导航焦点
                newState = UIState.Pressed;
            }
            else if (toggle != null && toggle.isOn)
            {
                // 选中（仅 toggle）
                newState = UIState.Selected;
            }
            else if (isHovered || isFocused)
            {
                // 鼠标悬浮/键盘手柄导航焦点
                newState = UIState.Hover;
            }
            else
            {
                // 待机
                newState = UIState.Normal;
            }

            // 如果状态发生变化，则执行表现更新
            if (newState != CurrentState)
            {
                CurrentState = newState;
                OnStateChanged.Invoke(CurrentState);
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            EvaluateState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            EvaluateState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            isPressed = true;
            EvaluateState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            isPressed = false;
            EvaluateState();
        }

        // 键盘/手柄导航事件视为鼠标悬浮
        public void OnSelect(BaseEventData eventData)
        {
            isFocused = true;
            EvaluateState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isFocused = false;
            EvaluateState();
        }
    }

    /// <summary>
    /// 提供 Unity 式可订阅的能力
    /// </summary>
    /// <example>
    /// selectable.OnStateChanged.AddListener(()=>{});
    /// </example>
    [Serializable]
    public class UIStateChangeEvent : UnityEvent<UIState>
    {
    }

    public enum UIState
    {
        // 越下方优先级越高，目前是用 if 手动判断优先级
        Normal, // 待机
        Hover, // 鼠标悬浮
        Selected, // 已选中 (仅Toggle)
        Pressed, // 鼠标按下&按住
        Disabled // 禁止交互
    }
}
