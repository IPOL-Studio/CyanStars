namespace CyanStars.Utils.RadioButton
{
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
            {
                button.Group = null;
#if UNITY_EDITOR
                UnityEngine.Debug.Log($"RadioButton {button.name} unregistered from group {group.name}");
#endif
            }
        }
    }
}
