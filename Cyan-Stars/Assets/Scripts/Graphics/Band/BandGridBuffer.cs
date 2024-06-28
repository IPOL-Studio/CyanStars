using UnityEngine;
using Random = UnityEngine.Random;

namespace CyanStars.Graphics.Band
{
    public class BandGridBuffer : MonoBehaviour
    {
        public Material Material;
        private ComputeBuffer computeBuffer;

        public void GenerateBuffer()
        {
            Release();
            float[] offsets = new float[100];
            for (int i = 0; i < 100; i++)
            {
                offsets[i] = Random.Range(0.0f, 0.5f);
            }

            computeBuffer = new ComputeBuffer(100, sizeof(float), ComputeBufferType.Default);
            computeBuffer.SetData(offsets);
            Material.SetBuffer("grid", computeBuffer);
        }

        public void Release()
        {
            if (computeBuffer != null)
            {
                computeBuffer.Release();
            }
        }
    }
}
