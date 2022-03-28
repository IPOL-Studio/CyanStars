using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldViewObject : ViewObject
{
    public MeshFilter meshFilter;
    public void SetMesh(float width, float length)
    {
        meshFilter.mesh = MeshHelper.CreateHoldMesh(width, length);
    }
    public void SetLength(float length)
    {
        var t = transform;
        var s = t.localScale;
        s.z = length;
        t.localScale = s;
    }
}
