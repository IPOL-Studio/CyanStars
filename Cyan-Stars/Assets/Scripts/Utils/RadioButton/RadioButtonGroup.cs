using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CyanStars.Utils.RadioButton
{
    public class RadioButtonGroup : UIBehaviour
    {
        public enum AddBehavior
        {
            SelectIfNoneSelected,
            Select,
            Deselect,
            //RespectItem
        }

        public enum CollectionChangeType
        {
            Add,
            Remove
        }

        private bool ignoreSelectionChange = false;
        private List<RadioButton> buttons = new List<RadioButton>();

        private RadioButton selectedItem;
        public RadioButton SelectedItem => selectedItem;

        [SerializeField]
        private AddBehavior itemAddBehavior = AddBehavior.SelectIfNoneSelected;

        [SerializeField]
        private bool allowSwitchOff = false;

        [SerializeField]
        private UnityEvent<RadioButton, RadioButton> onSelectedItemChanged = new UnityEvent<RadioButton, RadioButton>();

        [SerializeField]
        private UnityEvent<RadioButton, CollectionChangeType> onCollectionChanged = new UnityEvent<RadioButton, CollectionChangeType>();


        public AddBehavior ItemAddBehavior
        {
            get => itemAddBehavior;
            set => itemAddBehavior = value;
        }

        public bool AllowSwitchOff
        {
            get => allowSwitchOff;
            set
            {
                allowSwitchOff = value;
                EnsureValidState();
            }
        }

        public UnityEvent<RadioButton, RadioButton> OnSelectedItemChanged => onSelectedItemChanged;
        public UnityEvent<RadioButton, CollectionChangeType> OnCollectionChanged => onCollectionChanged;

        /// <summary>
        /// Only add a button to the group
        /// <para>If you want to set the group of a radio button, please set the <see cref="RadioButton.Group"/> or use the <see cref="RadioButtonGroupExtensions.Register"/></para>
        /// </summary>
        /// <param name="button"></param>
        internal void Add(RadioButton button)
        {
            if (button == null || buttons.Contains(button))
                return;

            buttons.Add(button);
            onCollectionChanged.Invoke(button, CollectionChangeType.Add);

            switch (itemAddBehavior)
            {
                case AddBehavior.SelectIfNoneSelected:
                    if (selectedItem == null)
                        SelectInGroup(button);
                    else if (button.IsChecked)
                        button.IsChecked = false;
                    break;
                case AddBehavior.Select:
                    SelectInGroup(button);
                    break;
                case AddBehavior.Deselect:
                    if (button.IsChecked)
                        button.IsChecked = false;
                    break;
                // case AddBehavior.RespectItem:
                //     break;
            }

            EnsureValidState();
        }

        /// <summary>
        /// Only remove a button from the group
        /// <para>If you want to unset the group of a radio button, please set the <see cref="RadioButton.Group"/> to null or use the <see cref="RadioButtonGroupExtensions.Unregister"/></para>
        /// </summary>
        internal bool Remove(RadioButton button)
        {
            if (button is null || !buttons.Remove(button))
                return false;

            if (selectedItem == button)
                selectedItem = null;

            EnsureValidState();
            onCollectionChanged.Invoke(button, CollectionChangeType.Remove);
            return true;
        }

        public void EnsureValidState()
        {
            if (allowSwitchOff) return;

            if (selectedItem != null && selectedItem.IsActive())
                return;

            for (int i = 0; i < buttons.Count; i++)
            {
                RadioButton item = buttons[i];
                if (item.IsInteractable() && item.IsActive())
                {
                    SelectInGroup(item);
                    return;
                }
            }
        }

        public void Select(RadioButton button)
        {
            if (button == null || !buttons.Contains(button))
                return;

            if (selectedItem == button)
                return;

            if (ignoreSelectionChange)
                return;

            ignoreSelectionChange = true;

            var old = selectedItem;
            selectedItem = button;
            old.IsChecked = false;

            ignoreSelectionChange = false;
            onSelectedItemChanged.Invoke(old, selectedItem);
        }

        private void SelectInGroup(RadioButton button)
        {
            if (button == SelectedItem)
                return;

            if (button.IsChecked)
            {
                Select(button);
                return;
            }

            button.IsChecked = true;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            EnsureValidState();
        }
#endif
    }

    public static class RadioButtonGroupExtensions
    {
        public static void Register(this RadioButtonGroup group, RadioButton button)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));
            if (button == null)
                throw new System.ArgumentNullException(nameof(button));

            button.Group = group;
        }

        public static void Unregister(this RadioButtonGroup group, RadioButton button)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));
            if (button == null)
                throw new System.ArgumentNullException(nameof(button));

            if (button.Group == group)
                button.Group = null;
        }
    }
}
