using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CyanStars.MarkdownRenderer.Editor
{
    [CustomPropertyDrawer(typeof(TextMeshProRenderConfig))]
    public class TextMeshProMarkdownConfigDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var content = new VisualElement();
            var endProperty = property.GetEndProperty();

            if (!property.NextVisible(true))
            {
                return content;
            }

            do
            {
                content.Add(new PropertyField(property));
            }
            while (property.NextVisible(false) && !SerializedProperty.EqualContents(property, endProperty));

            return content;
        }
    }
}