using UnityEngine;

namespace CyanStars.Render
{
    [ExecuteInEditMode]
    public class BoundsBox : MonoBehaviour
    {
        [SerializeField]
        private Transform transform;

        public Color Color1;
        public float Strength1;
        public Texture3D Texture3D;
        public Material Material;
        public new Camera Camera;
        private static readonly int BoundsMin = Shader.PropertyToID("_BoundsMin");
        private static readonly int BoundsMax = Shader.PropertyToID("_BoundsMax");
        private static readonly int Color = Shader.PropertyToID("_Color");
        // private static readonly int inverseProjectionMatrix = Shader.PropertyToID("_InverseProjectionMatrix");
        // private static readonly int inverseViewMatrix = Shader.PropertyToID("_InverseViewMatrix");
        private static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");

        void Update()
        {
            var localScale = transform.localScale;
            var position = transform.position;
            Material.SetVector(BoundsMin, position - localScale / 2);
            Material.SetVector(BoundsMax, position + localScale / 2);
            Material.SetVector(Color, Color1);
            // Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            // material.SetMatrix(inverseProjectionMatrix, projectionMatrix.inverse);
            // material.SetMatrix(inverseViewMatrix, camera.cameraToWorldMatrix);
            Material.SetTexture(NoiseTex, Texture3D);
        }
    }

}

