using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Linq;

using MyCurveField = UnityEditor.VFX.UI.VFXLabeledField<UnityEditor.UIElements.CurveField, UnityEngine.AnimationCurve>;

namespace UnityEditor.VFX.UI
{
    class CurvePropertyRM : PropertyRM<AnimationCurve>
    {
        public CurvePropertyRM(IPropertyRMProvider controller, float labelWidth) : base(controller, labelWidth)
        {
            m_CurveField = new MyCurveField(m_Label);
            m_CurveField.control.renderMode = CurveField.RenderMode.Mesh;
            m_CurveField.RegisterCallback<ChangeEvent<AnimationCurve>>(OnValueChanged);

            m_CurveField.style.flexDirection = FlexDirection.Column;
            m_CurveField.style.alignItems = Align.Stretch;
            m_CurveField.style.flexGrow = 1f;
            m_CurveField.style.flexShrink = 1f;

            Add(m_CurveField);
        }

        public override float GetPreferredControlWidth()
        {
            return 110;
        }

        public void OnValueChanged(ChangeEvent<AnimationCurve> e)
        {
            AnimationCurve newValue = m_CurveField.value;
            m_Value = newValue;
            NotifyValueChanged();
        }

        MyCurveField m_CurveField;

        protected override void UpdateEnabled()
        {
            m_CurveField.SetEnabled(propertyEnabled);
        }

        protected override void UpdateIndeterminate()
        {
            m_CurveField.visible = !indeterminate;
        }

        public override void UpdateGUI(bool force)
        {
            m_CurveField.SetValueWithoutNotify(m_Value);
        }

        public override bool showsEverything { get { return true; } }
    }
}
