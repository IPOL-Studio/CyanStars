using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework.Utils.JsonSerialization;
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
        /// <summary>
        /// 类型-自定义解析器 字典
        /// </summary>
        public static readonly Dictionary<Type, CustomRawAssetConverter> Converters =
            new Dictionary<Type, CustomRawAssetConverter>()
            {
                // 当 AssetsManager 解析这些类型时，将会自动用下面的解析器
                { typeof(ChartPackData), ChartPackDataConverter },
                { typeof(ChartData), ChartDataConverter },
                { typeof(AudioClip), AudioClipConverter }
            };

        private static object AudioClipConverter(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                // 使用 MemoryStream 将 byte[] 加载到内存中
                using var memoryStream = new MemoryStream(bytes);

                // 使用 NVorbis 的 VorbisReader 来读取 OGG 数据
                using var vorbisReader = new VorbisReader(memoryStream, true);

                // 获取音频信息
                var channels = vorbisReader.Channels;
                var sampleRate = vorbisReader.SampleRate;
                var totalSamples = (int)vorbisReader.TotalSamples;

                // 创建一个 float 数组来存放解码后的 PCM 数据
                var pcmData = new float[totalSamples * channels];

                // 读取所有样本
                vorbisReader.ReadSamples(pcmData, 0, pcmData.Length);

                // 创建 AudioClip
                AudioClip audioClip =
                    AudioClip.Create("ChartEditorAudioClip", totalSamples, channels, sampleRate, false);

                // 将解码后的 PCM 数据设置到 AudioClip 中
                audioClip.SetData(pcmData, 0);

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
        private static object ChartPackDataConverter(byte[] bytes)
        {
            return LoadJsonFromBytes<ChartPackData>(bytes);
        }

        /// <summary>
        /// 解析 ChartData 的包装函数
        /// </summary>
        private static object ChartDataConverter(byte[] bytes)
        {
            return LoadJsonFromBytes<ChartData>(bytes);
        }

        /// <summary>
        /// .json 通用解析器，用于将以 byte[] 形式的 .json 内容解析为任意兼容实例
        /// </summary>
        /// <param name="bytes">包含 Json 数据的字节数组 (应为 UTF-8 编码)</param>
        /// <returns>是否成功反序列化</returns>
        private static T LoadJsonFromBytes<T>(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogError("用于反序列化的 byte[] 为空或 null。");
                    return default;
                }

                // 设置反序列化格式参数
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    Culture = CultureInfo.InvariantCulture,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Converters = JsonConverters.Converters
                };

                // 将 byte[] 转换为 UTF-8 字符串
                string json = System.Text.Encoding.UTF8.GetString(bytes);

                T obj = JsonConvert.DeserializeObject<T>(json, settings);
                Debug.Log($"从 byte[] 反序列化完成。");
                return obj;
            }
            catch (Exception e)
            {
                Debug.LogError($"从 byte[] 反序列化时出现异常：{e}");
                return default;
            }
        }
    }
}
