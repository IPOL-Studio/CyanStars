#nullable enable

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils.RadioButton
{
    public class RadioButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField]
        private RadioButtonGroup? group;

        [SerializeField]
        private bool isChecked;

        [SerializeField]
        private UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

        [SerializeField]
        private UnityEvent<RadioButtonGroup?, RadioButtonGroup?> onGroupChanged = new UnityEvent<RadioButtonGroup?, RadioButtonGroup?>();


        public RadioButtonGroup? Group
        {
            get => group;
            set
            {
                if (group == value)
                    return;

                RadioButtonGroup? oldGroup = group;

                if (oldGroup != null)
                    oldGroup.Remove(this);

                group = value;

                if (group != null && IsActive())
                    group.Add(this);

                onGroupChanged.Invoke(oldGroup, value);
            }
        }

        public bool IsChecked
        {
            get => isChecked;
            set => SetIsChecked(value, true);
        }

        public UnityEvent<bool> OnValueChanged => onValueChanged;
        public UnityEvent<RadioButtonGroup?, RadioButtonGroup?> OnGroupChanged => onGroupChanged;


        private void SetIsChecked(bool value, bool isNotifyValueChanged)
        {
            if (isChecked == value)
                return;

            if (group == null)
            {
                isChecked = value;
                if (isNotifyValueChanged)
                    onValueChanged.Invoke(isChecked);
            }
            else
            {
                if (group.TrySetIsChecked(this, value))
                {
                    isChecked = value;
                    if (isNotifyValueChanged)
                        onValueChanged.Invoke(isChecked);
                }
            }
        }

        internal void SetIsCheckedByGroup(bool value)
        {
            if (isChecked == value)
                return;
            isChecked = value;
            onValueChanged.Invoke(value);
        }


        protected override void OnEnable()
        {
            if (group != null)
                group.Add(this);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (group != null)
                group.Remove(this);
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            transition = Transition.None;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (IsActive() && IsInteractable())
            {
                IsChecked = !IsChecked;
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable())
            {
                IsChecked = !IsChecked;
            }
        }

        // public void SetIsCheckedWithoutNotify(bool value)
        // {
        //     SetIsChecked(value, false);
        // }
    }
}
