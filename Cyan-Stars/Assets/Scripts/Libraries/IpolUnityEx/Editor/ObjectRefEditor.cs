using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Ipol.UnityEx.Editor
{
    [CustomPropertyDrawer(typeof(ObjectRef<>))]
    public class ObjectRefEditor : PropertyDrawer
    {
        private const string ValueName = ObjectRef<object>.ValueFieldName;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var value = property.FindPropertyRelative(ValueName);
            var foldout = new Foldout { value = property.isExpanded };
            var objectField = new ObjectField(property.displayName)
            {
                objectType = GetObjectType(),
                allowSceneObjects = true
            };
            ModifyObjectFieldStyle(objectField);
            var content = new VisualElement();

            objectField.BindProperty(value);
            AddObjectFieldToFoldoutHeader(foldout, objectField);
            foldout.Add(content);

            foldout.RegisterValueChangedCallback(change => property.isExpanded = change.newValue);
            objectField.RegisterValueChangedCallback(change => PopulateProperties(content, change.newValue));
            PopulateProperties(content, value.objectReferenceValue);

            return foldout;
        }

        private static void ModifyObjectFieldStyle(ObjectField objectField)
        {
            objectField.AddToClassList("unity-base-field__aligned");
            objectField.style.flexGrow = 1;
            objectField.style.marginLeft = 1;
            objectField.style.marginRight = 1;

            var label = objectField.labelElement;
            label.style.marginLeft = 0;
        }

        private static void AddObjectFieldToFoldoutHeader(Foldout foldout, ObjectField objectField)
        {
            var toggle = foldout.Q<Toggle>(className: Foldout.toggleUssClassName);
            toggle.Children().First().style.flexGrow = 0;
            toggle.Add(objectField);

            objectField.RegisterCallback<PointerDownEvent>(change => change.StopPropagation());
        }

        private static void PopulateProperties(VisualElement content, UnityObject referencedObject)
        {
            content.Unbind();
            content.Clear();

            if (referencedObject == null)
                return;

            var serializedObject = new SerializedObject(referencedObject);
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath != "m_Script")
                    content.Add(new PropertyField { bindingPath = iterator.propertyPath });
            }

            content.Bind(serializedObject);
        }

        private Type GetObjectType()
        {
            return fieldInfo.FieldType.IsGenericType
                ? fieldInfo.FieldType.GetGenericArguments()[0]
                : typeof(UnityObject);
        }

    }
}