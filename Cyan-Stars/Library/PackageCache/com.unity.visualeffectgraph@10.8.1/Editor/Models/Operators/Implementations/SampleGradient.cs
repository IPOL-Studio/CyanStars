using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.VFX.Operator
{
    [VFXInfo(category = "Sampling")]
    class SampleGradient : VFXOperator
    {
        override public string name { get { return "Sample Gradient"; } }

        public class InputProperties
        {
            [Tooltip("Sets the gradient to sample from.")]
            public Gradient gradient = VFXResources.defaultResources.gradient;
            [Range(0.0f, 1.0f), Tooltip("Sets the time along the gradient to take a sample from.")]
            public float time = 0.0f;
        }

        public class OutputProperties
        {
            [Tooltip("Outputs the sampled value from the gradient at the specified time.")]
            public Vector4 s = Vector4.zero;
        }

        protected override sealed VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            return new[] { new VFXExpressionSampleGradient(inputExpression[0], inputExpression[1]) };
        }
    }
}
