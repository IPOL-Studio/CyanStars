using System;
using UnityEngine;

namespace CyanStars.Graphics.Band
{
    public class Band : IDisposable
    {
        public struct BandData
        {
            public int count;
            public int xSize;
            public int ySize;
            public float xOffset;
            public float yOffset;
        }

        private bool isDisposed;
        private ComputeBuffer computeBuffer;

        public Band(BandData data)
        {
            computeBuffer = new ComputeBuffer(data.count, sizeof(float));
            Shader.SetGlobalBuffer("grid", computeBuffer);
            Shader.SetGlobalVector("_Aspect", new Vector4(data.xSize, data.ySize, data.xOffset, data.yOffset));
            Shader.SetGlobalInt("_Width", data.xSize / 2 - data.count);
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
