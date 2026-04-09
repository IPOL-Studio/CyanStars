using System;
using UnityEngine;

namespace CyanStars.Graphics.Band
{
    public class Band : IDisposable
    {
        private static readonly int GridId = Shader.PropertyToID("grid");
        private static readonly int AspectId = Shader.PropertyToID("_Aspect");
        private static readonly int WidthId = Shader.PropertyToID("_Width");

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

        // 现在正在设置 Band 的全局变量
        // 为避免冲突，先做个实际上只允许被持有一个的单例
        private static bool isCreated;

        private Band(BandData data)
        {
            computeBuffer = new ComputeBuffer(data.Count, sizeof(float));
            Shader.SetGlobalBuffer(GridId, computeBuffer);
            Shader.SetGlobalVector(AspectId, new Vector4(data.XSize, data.YSize, data.XOffset, data.YOffset));
            Shader.SetGlobalInt(WidthId, (data.XSize - data.Count) / 2);
        }

        public void UpdateBand(float[] bandHeights)
        {
            if (computeBuffer == null || bandHeights == null || computeBuffer.count != bandHeights.Length)
            {
                return;
            }
            computeBuffer.SetData(bandHeights);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                computeBuffer?.Release();
                isDisposed = true;
                isCreated = false;
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

        public static bool TryCreate(BandData data, out Band band)
        {
            if (isCreated)
            {
                band = null;
                return false;
            }

            band = new Band(data);
            isCreated = true;
            return true;
        }
    }
}
