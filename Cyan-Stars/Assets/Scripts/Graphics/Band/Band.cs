using System;
using UnityEngine;

namespace CyanStars.Graphics.Band
{
    public class Band : IDisposable
    {
        public struct BandData
        {
            public int Count;
            public int XSize;
            public int YSize;
            public float XOffset;
            public float YOffset;
        }

        private bool isDisposed;
        private ComputeBuffer computeBuffer;

        public Band(BandData data)
        {
            computeBuffer = new ComputeBuffer(data.Count + 1, sizeof(float));
            Shader.SetGlobalBuffer("grid", computeBuffer);
            Shader.SetGlobalVector("_Aspect", new Vector4(data.XSize, data.YSize, data.XOffset, data.YOffset));
            Shader.SetGlobalInt("_Width", (data.XSize - data.Count) / 2);
        }

        public void UpdateBand(float[] bandHeights)
        {
            if (computeBuffer == null || computeBuffer.count != bandHeights.Length)
            {
                return;
            }
            computeBuffer.SetData(bandHeights);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                computeBuffer.Release();
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Band()
        {
            Dispose(false);
        }
    }
}
