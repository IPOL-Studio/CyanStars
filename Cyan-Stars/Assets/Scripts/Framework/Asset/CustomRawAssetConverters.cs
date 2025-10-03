using System;
using System.Collections.Generic;
using System.Globalization;
using CatAsset.Runtime;
using CyanStars.Chart;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 自定义原生资源转换器
    /// </summary>
    public static class CustomRawAssetConverters
    {
        /// <summary>
        /// 允许哪些类型通过 .json 解析
        /// </summary>
        public static readonly Dictionary<Type, CustomRawAssetConverter> Converters =
            new Dictionary<Type, CustomRawAssetConverter>()
            {
                { typeof(ChartPackData), ChartPackDataConverter }, // 当 AssetsManager 解析 ChartPackData 时，将会自动用下面的解析器
                { typeof(ChartData), ChartDataConverter }
            };

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
        /// .json 通用解析器，用于将以 byte[] 形式的 .json 内容解析为任意实例
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
                    Converters = JsonConverters
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

        private static readonly IList<JsonConverter> JsonConverters = new List<JsonConverter>
        {
            new ColorConverter(), new ChartNoteDataReadConverter(), new ChartTrackDataReadConverter()
        };
    }
}
