#nullable enable

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils.SelectableUI
{
    public enum UIState
    {
        // 越下方优先级越高，目前是用 if 手动判断优先级
        Normal, // 待机
        NormalSelected, // 待机+已选中 (仅Toggle)
        Hover, // 鼠标悬浮
        HoverSelected, // 鼠标悬浮+已选中 (仅Toggle)
        Pressed, // 鼠标按下&按住
        PressedSelected, // 鼠标按下&按住+已选中 (仅Toggle)
        Disabled, // 禁止交互
        DisabledSelected // 禁止交互+已选中 (仅Toggle)
    }

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
    /// 4. 再写一个脚本监听并处理本脚本的 OnStateChanged 视图事件（参见 SelectableViewEffectHandler）
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Selectable))]
    public class SelectableStateObserver : UIBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        public UIState CurrentState { get; private set; } = UIState.Normal;


        [Header("状态改变回调")]
        [SerializeField]
        private UnityEvent<UIState> onStateChanged;

        public UnityEvent<UIState> OnStateChanged => onStateChanged;

        private Selectable selectable = null!;

        private bool isKeepSelected; // 被选中（如果组件是 Toggle，将自动从 toggle.isOn 同步）
        private bool isHovered; // 鼠标悬浮
        private bool isFocused; // 键盘/手柄导航焦点
        private bool isPressed; // 按住


        protected override void Awake()
        {
            base.Awake();

            selectable = GetComponent<Selectable>();

            if (selectable.transition != Selectable.Transition.None)
            {
                Debug.LogWarning($"{nameof(SelectableStateObserver)} 已自动禁用 selectable.transition，请检查是否为预期效果。", gameObject);
                selectable.transition = Selectable.Transition.None;
            }
        }

        protected override void OnEnable()
        {
            if (selectable is Toggle toggle)
            {
                isKeepSelected = toggle.isOn;
                toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
            else
            {
                isKeepSelected = false;
            }

            isHovered = false;
            isFocused = false;
            isPressed = false;
            EvaluateState();
        }

        protected override void OnDisable()
        {
            if (selectable is Toggle toggle)
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);

            isKeepSelected = false;
            isHovered = false;
            isFocused = false;
            isPressed = false;
            CurrentState = UIState.Normal;
        }


        /// <summary>
        /// 外部手动修改组件可交互性，同时一并触发视觉效果更新
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            // 考虑到 UI 内可能有大量 SelectableStateObserver 组件，故不采用 Update() 自动轮询
            selectable.interactable = interactable;
            EvaluateState();
        }


        protected override void OnCanvasGroupChanged()
        {
            base.OnCanvasGroupChanged();
            EvaluateState();
        }

        /// <summary>
        /// 当 selectable 是 toggle 时，外界改变 toggle.isOn 时也会自动改变视觉效果
        /// </summary>
        private void OnToggleValueChanged(bool value)
        {
            if (isKeepSelected == value)
                return;
            isKeepSelected = value;
            EvaluateState();
        }

        private void EvaluateState()
        {
            UIState newState;

            if (!selectable.IsInteractable())
            {
                // 禁止交互
                newState = isKeepSelected ? UIState.DisabledSelected : UIState.Disabled;
            }
            else if (isPressed && (isHovered || isFocused))
            {
                // 按下，且鼠标不移出组件区域/是键盘手柄导航焦点
                newState = isKeepSelected ? UIState.PressedSelected : UIState.Pressed;
            }
            else if (isHovered || isFocused)
            {
                // 鼠标悬浮/键盘手柄导航焦点
                newState = isKeepSelected ? UIState.HoverSelected : UIState.Hover;
            }
            else
            {
                // 待机
                newState = isKeepSelected ? UIState.NormalSelected : UIState.Normal;
            }

            // 如果状态发生变化，则执行表现更新
            if (newState != CurrentState)
            {
                CurrentState = newState;
                onStateChanged.Invoke(CurrentState);
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
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPressed = true;
            EvaluateState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            isPressed = false;

            // 抬起鼠标时清除焦点，防止卡在 Hover 状态
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
                EventSystem.current.SetSelectedGameObject(null);

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
}
