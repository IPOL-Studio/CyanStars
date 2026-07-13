#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class AudioWaveformGenerator : IDisposable
    {
        private const int AudioWaveTextureWidthPixel = 32;
        private const float AudioWaveSamplesPerSecond = 10f;
        private readonly Color32 AudioWaveColor = new Color32(255, 255, 255, 13);
        private readonly Color32 ClearColor = new Color32(0, 0, 0, 0);

        // 缓存的预计算数据
        private NativeArray<float> precomputedSamples;
        private int cachedMusicOffset;
        private AudioClip? cachedClip;
        private Texture2D? currentTexture;

        public void Dispose()
        {
            if (precomputedSamples.IsCreated)
            {
                precomputedSamples.Dispose();
            }
            if (currentTexture != null)
            {
                UnityEngine.Object.Destroy(currentTexture);
            }
        }

        /// <summary>
        /// 预计算音频数据
        /// </summary>
        public async Task PrecomputeAsync(AudioClip? clip, int musicOffset, CancellationToken token)
        {
            if (clip == cachedClip && musicOffset == cachedMusicOffset)
                return; // 数据未变，无需重新预计算

            cachedClip = clip;
            cachedMusicOffset = musicOffset;

            if (precomputedSamples.IsCreated)
                precomputedSamples.Dispose();

            if (clip == null)
                return;

            int clipSamples = clip.samples;
            int clipChannels = clip.channels;
            int clipFrequency = clip.frequency;
            float clipLength = clip.length;

            float[] fullSamples = new float[clipSamples * clipChannels];
            clip.GetData(fullSamples, 0);

            // 异步执行降采样计算
            await Task.Run(() =>
            {
                float musicOffsetS = musicOffset / 1000f;
                float timelineTotalTimeS = math.max(0, clipLength + musicOffsetS);
                int samplesCount = (int)math.ceil(timelineTotalTimeS * AudioWaveSamplesPerSecond);

                NativeArray<float> rawSamples = new NativeArray<float>(fullSamples, Allocator.Persistent);
                NativeArray<float> downsampled = new NativeArray<float>(samplesCount, Allocator.Persistent);

                try
                {
                    var downsampleJob = new DownsampleAudioJob
                    {
                        FullSamples = rawSamples,
                        Channels = clipChannels,
                        Frequency = clipFrequency,
                        ClipSamples = clipSamples,
                        MusicOffsetS = musicOffsetS,
                        TimelineTotalTimeS = timelineTotalTimeS,
                        SamplesCount = samplesCount,
                        Result = downsampled
                    };

                    downsampleJob.Schedule(samplesCount, 64).Complete();

                    // 找出最大值
                    float maxAmp = 0.0001f;
                    for (int i = 0; i < samplesCount; i++)
                    {
                        if (downsampled[i] > maxAmp) maxAmp = downsampled[i];
                    }

                    // 归一化并缓存
                    precomputedSamples = new NativeArray<float>(samplesCount, Allocator.Persistent);
                    for (int i = 0; i < samplesCount; i++)
                    {
                        precomputedSamples[i] = downsampled[i] / maxAmp;
                    }
                }
                finally
                {
                    rawSamples.Dispose();
                    downsampled.Dispose();
                }
            }, token);
        }

        /// <summary>
        /// 异步生成纹理
        /// </summary>
        public async Task<Texture2D?> GenerateTextureAsync(int uiHeight, CancellationToken token)
        {
            if (!precomputedSamples.IsCreated || precomputedSamples.Length == 0 || uiHeight <= 0)
                return null;

            int maxTexSize = math.min(SystemInfo.maxTextureSize, 8192);
            int texHeight = math.clamp(uiHeight, 1, maxTexSize);
            int totalPixels = AudioWaveTextureWidthPixel * texHeight;

            // 创建接收像素数据的 NativeArray
            NativeArray<Color32> pixels = new NativeArray<Color32>(totalPixels, Allocator.Persistent);

            try
            {
                var drawJob = new DrawWaveformTextureJob
                {
                    Samples = precomputedSamples,
                    TexWidth = AudioWaveTextureWidthPixel,
                    TexHeight = texHeight,
                    WaveColor = AudioWaveColor,
                    BgColor = ClearColor,
                    Pixels = pixels
                };

                await Task.Run(() =>
                {
                    drawJob.Schedule(totalPixels, 64).Complete();
                }, token);

                if (token.IsCancellationRequested) return null;

                if (currentTexture != null && (currentTexture.width != AudioWaveTextureWidthPixel || currentTexture.height != texHeight))
                {
                    UnityEngine.Object.Destroy(currentTexture);
                    currentTexture = null;
                }

                if (currentTexture == null)
                {
                    currentTexture = new Texture2D(AudioWaveTextureWidthPixel, texHeight, TextureFormat.RGBA32, false);
                }

                currentTexture.SetPixelData(pixels, 0);
                currentTexture.Apply();

                return currentTexture;
            }
            finally
            {
                pixels.Dispose();
            }
        }

        #region Burst Jobs

        [BurstCompile]
        private struct DownsampleAudioJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> FullSamples;
            public int Channels;
            public int Frequency;
            public int ClipSamples;
            public float MusicOffsetS;
            public float TimelineTotalTimeS;
            public int SamplesCount;

            [WriteOnly] public NativeArray<float> Result;

            public void Execute(int i)
            {
                float t0 = (float)i / SamplesCount * TimelineTotalTimeS;
                float t1 = (float)(i + 1) / SamplesCount * TimelineTotalTimeS;

                float localT0 = t0 - MusicOffsetS;
                float localT1 = t1 - MusicOffsetS;

                int frameStart = math.clamp((int)(localT0 * Frequency), 0, ClipSamples);
                int frameEnd = math.clamp((int)(localT1 * Frequency), 0, ClipSamples);

                float maxAmplitude = 0f;
                if (frameStart < frameEnd)
                {
                    for (int f = frameStart; f < frameEnd; f++)
                    {
                        int baseIndex = f * Channels;
                        for (int ch = 0; ch < Channels; ch++)
                        {
                            float amplitude = math.abs(FullSamples[baseIndex + ch]);
                            if (amplitude > maxAmplitude)
                            {
                                maxAmplitude = amplitude;
                            }
                        }
                    }
                }
                Result[i] = maxAmplitude;
            }
        }

        [BurstCompile]
        private struct DrawWaveformTextureJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float> Samples;
            public int TexWidth;
            public int TexHeight;
            public Color32 WaveColor;
            public Color32 BgColor;

            [WriteOnly] public NativeArray<Color32> Pixels;

            // index 代表当前的像素索引 (0 到 totalPixels-1)
            public void Execute(int index)
            {
                // 将一维索引转为 2D 的 y(行) 和 x(列)
                int y = index / TexWidth;
                int x = index % TexWidth;

                // 按照当前所在的行 (y) 计算采样的振幅
                float t = TexHeight > 1 ? (float)y / (TexHeight - 1) : 0f;
                int sampleIndex = math.clamp((int)math.round(t * (Samples.Length - 1)), 0, Samples.Length - 1);
                float amplitude = Samples[sampleIndex];

                // 判断当前像素 (x) 是否在振幅宽度范围内
                float centerX = (TexWidth - 1) / 2f;
                float maxHalfWidth = TexWidth / 2f;
                float halfWidth = amplitude * maxHalfWidth;
                float dist = math.abs(x - centerX);

                Pixels[index] = (dist <= halfWidth) ? WaveColor : BgColor;
            }
        }

        #endregion

    }
}
