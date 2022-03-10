using System;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    [VFXInfo(type = typeof(Vector3))]
    class VFXSlotFloat3 : VFXSlot
    {
        static public bool CanConvertFromVector3(Type type)
        {
            return type == typeof(Vector3)
                || type == typeof(float)
                || type == typeof(uint)
                || type == typeof(int)
                || type == typeof(Vector4)
                || type == typeof(Color)
                || type == typeof(Vector)
                || type == typeof(Position)
                || type == typeof(DirectionType);
        }

        sealed protected override bool CanConvertFrom(Type type)
        {
            return base.CanConvertFrom(type)
                || CanConvertFromVector3(type);
        }

        sealed public override VFXValue DefaultExpression(VFXValue.Mode mode)
        {
            return new VFXValue<Vector3>(Vector3.zero, mode);
        }

        static public VFXExpression ConvertExpressionToVector3(VFXExpression expression)
        {
            if (expression.valueType == VFXValueType.Float3)
                return expression;

            if (expression.valueType == VFXValueType.Float)
                return new VFXExpressionCombine(expression, expression, expression);

            if (expression.valueType == VFXValueType.Uint32)
            {
                var floatExpression = new VFXExpressionCastUintToFloat(expression);
                return new VFXExpressionCombine(floatExpression, floatExpression, floatExpression);
            }

            if (expression.valueType == VFXValueType.Int32)
            {
                var floatExpression = new VFXExpressionCastIntToFloat(expression);
                return new VFXExpressionCombine(floatExpression, floatExpression, floatExpression);
            }

            if (expression.valueType == VFXValueType.Float4)
            {
                return new VFXExpressionCombine(expression.x, expression.y, expression.z);
            }

            throw new Exception("Unexpected type of expression " + expression + "valueType" + expression.valueType);
        }

        sealed protected override VFXExpression ConvertExpression(VFXExpression expression, VFXSlot sourceSlot)
        {
            return ConvertExpressionToVector3(expression);
        }

        sealed protected override VFXExpression ExpressionFromChildren(VFXExpression[] expr)
        {
            return new VFXExpressionCombine(
                expr[0],
                expr[1],
                expr[2]);
        }

        sealed protected override VFXExpression[] ExpressionToChildren(VFXExpression expr)
        {
            return new VFXExpression[3]
            {
                expr.x,
                expr.y,
                expr.z
            };
        }
    }
}
