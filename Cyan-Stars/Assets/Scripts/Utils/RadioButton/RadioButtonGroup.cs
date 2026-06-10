#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CyanStars.Utils.RadioButton
{
    public class RadioButtonGroup : UIBehaviour, IEnumerable<RadioButtonItem>
    {
        public enum CollectionChangeType
        {
            Add,
            Remove
        }

        [SerializeField]
        private bool allowSwitchOff;

        [SerializeField]
        private UnityEvent<RadioButtonItem?, RadioButtonItem?> onSelectedItemChanged = new UnityEvent<RadioButtonItem?, RadioButtonItem?>();

        [SerializeField]
        private UnityEvent<RadioButtonItem, CollectionChangeType> onCollectionChanged = new UnityEvent<RadioButtonItem, CollectionChangeType>();


        public bool AllowSwitchOff
        {
            get => allowSwitchOff;
            set
            {
                allowSwitchOff = value;
                EnsureValidState();
            }
        }

        public RadioButtonItem? SelectedItem { get; private set; }
        public UnityEvent<RadioButtonItem?, RadioButtonItem?> OnSelectedItemChanged => onSelectedItemChanged;
        public UnityEvent<RadioButtonItem, CollectionChangeType> OnCollectionChanged => onCollectionChanged;

        private readonly List<RadioButtonItem> Buttons = new List<RadioButtonItem>();


        /// <summary>
        /// Only add a button to the group
        /// <para>If you want to set the group of a radio button, please set the <see cref="RadioButtonItem.Group"/> or use the <see cref="RadioButtonGroupExtensions.Register"/></para>
        /// </summary>
        internal void Add(RadioButtonItem button)
        {
            if (button == null || Buttons.Contains(button))
                return;

            Buttons.Add(button);

            if (button.IsChecked)
            {
                if (SelectedItem == null)
                {
                    SelectedItem = button;
                }
                else if (SelectedItem != button)
                {
                    button.SetIsCheckedByGroup(false);
                }
            }

            onCollectionChanged.Invoke(button, CollectionChangeType.Add);
            EnsureValidState();
        }

        /// <summary>
        /// Only remove a button from the group
        /// <para>If you want to unset the group of a radio button, please set the <see cref="RadioButtonItem.Group"/> to null or use the <see cref="RadioButtonGroupExtensions.Unregister"/></para>
        /// </summary>
        internal bool Remove(RadioButtonItem button)
        {
            if (button is null || !Buttons.Remove(button))
                return false;

            if (SelectedItem == button)
            {
                SelectedItem = null;
                onSelectedItemChanged.Invoke(button, null);
            }

            EnsureValidState();
            onCollectionChanged.Invoke(button, CollectionChangeType.Remove);
            return true;
        }

        public void EnsureValidState()
        {
            if (allowSwitchOff)
                return;

            if (SelectedItem != null && SelectedItem.IsActive())
                return;

            for (int i = 0; i < Buttons.Count; i++)
            {
                RadioButtonItem item = Buttons[i];
                if (item.IsInteractable() && item.IsActive())
                {
                    item.IsChecked = true;
                    return;
                }
            }
        }

        public bool TrySetIsChecked(RadioButtonItem button, bool targetValue)
        {
            if (button == null || !Buttons.Contains(button))
                return false;

            var old = SelectedItem;

            if (!targetValue)
            {
                // -- 处理取消选中 --
                if (!allowSwitchOff)
                    return false;

                if (old == button)
                {
                    SelectedItem = null;
                    onSelectedItemChanged.Invoke(old, null);
                }

                return true;
            }
            else
            {
                // -- 处理选中 --
                if (old == button)
                    return false;

                if (old != null)
                {
                    old.SetIsCheckedByGroup(false);
                }

                SelectedItem = button;
                onSelectedItemChanged.Invoke(old, SelectedItem);
                return true;
            }
        }

        public IEnumerator<RadioButtonItem> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
}
