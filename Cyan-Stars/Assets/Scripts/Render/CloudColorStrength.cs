using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Scene/Cloud")]
public class CloudColorStrength : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Color")]
    public ColorParameter color = new ColorParameter(Color.white);

    [Range(0, 1f), Tooltip("Strength")]
    public FloatParameter strength = new FloatParameter(0.5f);

    public bool IsActive() => active && strength.value > 0f;
    public bool IsTileCompatible() => false;
}
