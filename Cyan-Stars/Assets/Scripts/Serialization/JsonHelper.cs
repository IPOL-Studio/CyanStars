using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CyanStars.Gameplay.Chart;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Serialization
{
    /// <summary>
    /// 序列化和反序列化工具类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 自定义 JsonConverter 列表
        /// </summary>
        private static readonly IList<JsonConverter> Converters = new List<JsonConverter>
        {
            new ColorConverter(), new ChartNoteDataReadConverter(), new ChartTrackDataReadConverter()
        };


        /// <summary>
        /// 序列化对象为 Json 文件
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="filePath">保存到的路径和文件全名</param>
        /// <returns>是否成功序列化</returns>
        public static bool ToJson(object obj, string filePath)
        {
            try
            {
                // 设置序列化格式参数
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    Culture = CultureInfo.InvariantCulture,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Converters = Converters
                };

                // 如果目录不存在，创建目录
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(obj, settings);
                File.WriteAllText(filePath, json);
                Debug.Log($"序列化完成，文件路径：{filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化时出现异常：{e}");
                return false;
            }
        }

        /// <summary>
        /// 从 Json 文件反序列化为对象
        /// </summary>
        /// <param name="filePath">要读取的文件路径</param>
        /// <param name="obj">输出的反序列化对象，若成功返回反序列化的对象，若失败则为默认值</param>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>是否成功反序列化</returns>
        public static bool FromJson<T>(string filePath, [CanBeNull] out T obj)
        {
            obj = default;
            try
            {
                // 设置反序列化格式参数
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    Culture = CultureInfo.InvariantCulture,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Converters = Converters
                };

                if (!File.Exists(filePath))
                {
                    Debug.LogError($"未找到需要反序列化的文件：{filePath}");
                    return false;
                }

                string json = File.ReadAllText(filePath);
                obj = JsonConvert.DeserializeObject<T>(json, settings);
                Debug.Log($"反序列化完成，从文件：{filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"反序列化时出现异常：{e}");
                return false;
            }
        }
    }
}
