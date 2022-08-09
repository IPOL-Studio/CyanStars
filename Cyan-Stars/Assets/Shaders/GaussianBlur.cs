using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    [SerializableAttribute, VolumeComponentMenu("Custom-Post-Processing/GaussianBlue")]
    public class GaussianBlur : VolumeComponent, IPostProcessComponent
    {
        public ClampedIntParameter blurCount = new ClampedIntParameter(1, 1, 4);
        public ClampedIntParameter downSample = new ClampedIntParameter(1, 1, 4);
        public ClampedFloatParameter indensity = new ClampedFloatParameter(0f, 0f, 20f);

        public bool IsActive()
        {
            return active && indensity.value != 0;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}