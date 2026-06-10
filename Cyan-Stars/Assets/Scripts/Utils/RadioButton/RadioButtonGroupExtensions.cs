#nullable enable

using UnityEngine;

namespace CyanStars.Utils.RadioButton
{
    public static class RadioButtonGroupExtensions
    {
        public static void Register(this RadioButtonGroup group, RadioButtonItem button)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));
            if (button == null)
                throw new System.ArgumentNullException(nameof(button));

            button.Group = group;
        }

        public static void Unregister(this RadioButtonGroup group, RadioButtonItem button)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));
            if (button == null)
                throw new System.ArgumentNullException(nameof(button));

            if (button.Group == group)
            {
                button.Group = null;
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"Unregister failed, RadioButtonItem {button.name} do not belong to group {group.name}");
            }
#endif
        }
    }
}
