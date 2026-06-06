using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils.RadioButton
{
    public class RadioButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField]
        private RadioButtonGroup group;

        [SerializeField]
        private bool isChecked;


        public RadioButtonGroup Group
        {
            get => group;
            set
            {
                if (group == value)
                    return;

                if (group != null)
                    group.Remove(this);

                group = value;

                if (group != null && IsActive())
                    group.Add(this);

                OnGroupChanged.Invoke(group, value);
            }
        }

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (isChecked == value)
                    return;

                isChecked = value;

                if (isChecked && group != null)
                    group.Select(this);

                OnValueChanged.Invoke(isChecked);
            }
        }

        private UnityEvent<bool> onValueChanged = new UnityEvent<bool>();
        private UnityEvent<RadioButtonGroup, RadioButtonGroup> onGroupChanged = new UnityEvent<RadioButtonGroup, RadioButtonGroup>();

        public UnityEvent<bool> OnValueChanged => onValueChanged;
        public UnityEvent<RadioButtonGroup, RadioButtonGroup> OnGroupChanged => onGroupChanged;

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
                IsChecked = true;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable())
                IsChecked = true;
        }
    }
}
