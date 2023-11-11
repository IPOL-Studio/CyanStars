using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CyanStars.EditorExtension
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ActiveAttribute : PropertyAttribute
    {
        public readonly ActiveMode Mode;

        public ActiveAttribute(ActiveMode mode)
        {
            Mode = mode;
        }
    }

    [System.Flags]
    public enum ActiveMode
    {
        Edit = 1,
        Playing = 2,
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActiveAttribute))]
    internal class ActiveDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isActive = IsActive(((ActiveAttribute)attribute).Mode);
            bool isEnable = GUI.enabled;

            if (!isActive)
                GUI.enabled = false;

            EditorGUI.PropertyField(position, property, label);

            GUI.enabled = isEnable;
        }

        private bool IsActive(ActiveMode mode)
        {
            ActiveMode currentMode = Application.isPlaying ? ActiveMode.Playing : ActiveMode.Edit;
            return (currentMode & mode) > 0;
        }
    }
#endif
}
