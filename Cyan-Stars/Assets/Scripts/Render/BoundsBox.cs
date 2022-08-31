using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoundsBox : MonoBehaviour
{
    [SerializeField]
    private Transform transform;

    public Color color1;
    public float strength1;
    public Texture3D texture3D;
    public Material material;
    public Camera camera;
    private static readonly int boundsMin = Shader.PropertyToID("_BoundsMin");
    private static readonly int boundsMax = Shader.PropertyToID("_BoundsMax");
    private static readonly int color = Shader.PropertyToID("_Color");
    // private static readonly int inverseProjectionMatrix = Shader.PropertyToID("_InverseProjectionMatrix");
    // private static readonly int inverseViewMatrix = Shader.PropertyToID("_InverseViewMatrix");
    private static readonly int noiseTex = Shader.PropertyToID("_NoiseTex");

    void Update()
    {
        var localScale = transform.localScale;
        var position = transform.position;
        material.SetVector(boundsMin, position - localScale / 2);
        material.SetVector(boundsMax, position + localScale / 2);
        material.SetVector(color, color1);
        // Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        // material.SetMatrix(inverseProjectionMatrix, projectionMatrix.inverse);
        // material.SetMatrix(inverseViewMatrix, camera.cameraToWorldMatrix);
        material.SetTexture(noiseTex, texture3D);
    }
}
