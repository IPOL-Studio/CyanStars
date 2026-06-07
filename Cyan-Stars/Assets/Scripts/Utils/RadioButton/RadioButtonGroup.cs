#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CyanStars.Utils.RadioButton
{
    public class RadioButtonGroup : UIBehaviour
    {
        public enum CollectionChangeType
        {
            Add,
            Remove
        }

        public enum AllowSwitchOffType
        {
            Always, // 可以取消选中所有
            ScriptOnly, // 玩家无法取消选中，但脚本添加未选中的 button / 取消选中 button 不受影响
            Never // 不能取消选中所有
        }


        [SerializeField]
        private AllowSwitchOffType allowSwitchOff;

        [SerializeField]
        private UnityEvent<RadioButton?, RadioButton?> onSelectedItemChanged = new UnityEvent<RadioButton?, RadioButton?>();

        [SerializeField]
        private UnityEvent<RadioButton, CollectionChangeType> onCollectionChanged = new UnityEvent<RadioButton, CollectionChangeType>();


        public AllowSwitchOffType AllowSwitchOff
        {
            get => allowSwitchOff;
            set
            {
                allowSwitchOff = value;
                EnsureValidState();
            }
        }

        public RadioButton? SelectedItem { get; private set; }
        public UnityEvent<RadioButton?, RadioButton?> OnSelectedItemChanged => onSelectedItemChanged;
        public UnityEvent<RadioButton, CollectionChangeType> OnCollectionChanged => onCollectionChanged;

        private bool uncheckByGroup = false;
        private readonly List<RadioButton> Buttons = new List<RadioButton>();


        /// <summary>
        /// Only add a button to the group
        /// <para>If you want to set the group of a radio button, please set the <see cref="RadioButton.Group"/> or use the <see cref="RadioButtonGroupExtensions.Register"/></para>
        /// </summary>
        internal void Add(RadioButton button)
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
                    button.IsChecked = false;
                }
            }

            onCollectionChanged.Invoke(button, CollectionChangeType.Add);
            EnsureValidState();
        }

        /// <summary>
        /// Only remove a button from the group
        /// <para>If you want to unset the group of a radio button, please set the <see cref="RadioButton.Group"/> to null or use the <see cref="RadioButtonGroupExtensions.Unregister"/></para>
        /// </summary>
        internal bool Remove(RadioButton button)
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
            if (allowSwitchOff == AllowSwitchOffType.Always)
                return;

            if (SelectedItem != null && SelectedItem.IsActive())
                return;

            for (int i = 0; i < Buttons.Count; i++)
            {
                RadioButton item = Buttons[i];
                if (item.IsInteractable() && item.IsActive())
                {
                    item.IsChecked = true;
                    return;
                }
            }
        }

        public bool TrySetCheckedState(RadioButton button, bool targetValue, bool uncheckByPlayer)
        {
            if (button == null || !Buttons.Contains(button))
                return false;

            if (uncheckByGroup)
                return true;

            var old = SelectedItem;

            if (!targetValue)
            {
                // -- 处理取消选中 --
                if (allowSwitchOff == AllowSwitchOffType.Never)
                    return false;
                if (allowSwitchOff == AllowSwitchOffType.ScriptOnly && uncheckByPlayer)
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
                    return true;

                uncheckByGroup = true;
                if (old != null)
                {
                    old.IsChecked = false;
                }

                uncheckByGroup = false;

                SelectedItem = button;
                onSelectedItemChanged.Invoke(old, SelectedItem);
                return true;
            }
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
