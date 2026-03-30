using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Utils.JsonSerialization;
using Newtonsoft.Json;
using UnityEngine;
using NVorbis;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 自定义原生资源转换器
    /// </summary>
    public static class CustomRawAssetConverters
    {
        public static void Register()
        {
            CatAssetManager.RegisterAsyncCustomRawAssetConverter(AudioClipConverter);
            CatAssetManager.RegisterCustomRawAssetConverter(ChartPackDataConverter);
            // CatAssetManager.RegisterCustomRawAssetConverter(ChartDataConverter);
        }


        private static async Task<AudioClip> AudioClipConverter(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                var audioData =
                    await Task.Run(() =>
                        {
                            // 使用 MemoryStream 将 byte[] 加载到内存中
                            using var memoryStream = new MemoryStream(bytes);

                            // 使用 NVorbis 的 VorbisReader 来读取 OGG 数据
                            using var vorbisReader = new VorbisReader(memoryStream, true);

                            // 获取音频信息
                            var channels = vorbisReader.Channels;
                            var sampleRate = vorbisReader.SampleRate;
                            var totalSamples = (int)vorbisReader.TotalSamples;

                            // 创建一个 float[] 来存放解码后的 PCM 数据
                            var pcmData = new float[totalSamples * channels];

                            // 读取所有样本
                            vorbisReader.ReadSamples(pcmData, 0, pcmData.Length);

                            // 返回解码后的数据
                            return (channels, sampleRate, totalSamples, pcmData);
                        }
                    );

                // 创建 AudioClip
                AudioClip audioClip = AudioClip.Create(
                    "ChartEditorAudioClip",
                    audioData.totalSamples,
                    audioData.channels,
                    audioData.sampleRate,
                    false
                );

                // 将解码后的 PCM 数据设置到 AudioClip 中
                audioClip.SetData(audioData.pcmData, 0);

                return audioClip;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to convert OGG to AudioClip: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析 ChartPackData 的包装函数
        /// </summary>
        private static ChartPackData ChartPackDataConverter(byte[] bytes)
        {
            return JsonLoadHelper.LoadData<ChartPackData>(bytes);
        }

        // /// <summary>
        // /// 解析 ChartData 的包装函数
        // /// </summary>
        // private static ChartData ChartDataConverter(byte[] bytes)
        // {
        //     return JsonLoadHelper.LoadData<ChartData>(bytes);
        // }
    }
}
