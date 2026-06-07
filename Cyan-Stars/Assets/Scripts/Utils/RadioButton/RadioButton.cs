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
            set
            {
                if (isChecked == value)
                    return;

                if (group == null)
                {
                    isChecked = value;
                    if (!noValueChangedNotice)
                    {
                        onValueChanged.Invoke(isChecked);
                    }
                }
                else
                {
                    if (group.TrySetCheckedState(this, value, isChangeByPlayer))
                    {
                        isChecked = value;
                        if (!noValueChangedNotice)
                        {
                            onValueChanged.Invoke(isChecked);
                        }
                    }
                }
            }
        }

        public UnityEvent<bool> OnValueChanged => onValueChanged;
        public UnityEvent<RadioButtonGroup?, RadioButtonGroup?> OnGroupChanged => onGroupChanged;


        private bool isChangeByPlayer = false;
        private bool noValueChangedNotice = false;


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
                isChangeByPlayer = true;
                IsChecked = !IsChecked;
                isChangeByPlayer = false;
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable())
            {
                isChangeByPlayer = true;
                IsChecked = !IsChecked;
                isChangeByPlayer = false;
            }
        }

        public void SetCheckedWithoutNotify(bool value)
        {
            noValueChangedNotice = true;
            try
            {
                IsChecked = value;
            }
            finally
            {
                noValueChangedNotice = false;
            }
        }
    }
}
