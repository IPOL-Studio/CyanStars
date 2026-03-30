#nullable enable

using System;
using System.Globalization;
using CyanStars.Utils.JsonSerialization;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// .json 通用解析器，用于将以 byte[] 或 string 形式的 json 内容解析为任意兼容实例
    /// </summary>
    public static class JsonLoadHelper
    {
        // 反序列化格式参数
        private static readonly JsonSerializerSettings Settings = new()
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented,
            Culture = CultureInfo.InvariantCulture,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Converters = JsonConverters.Converters
        };

        public static T? LoadData<T>(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                Debug.LogError("用于反序列化的 byte[] 为空或 null。");
                return default;
            }

            // 将 byte[] 转换为 UTF-8 字符串
            string str = System.Text.Encoding.UTF8.GetString(bytes);
            return LoadData<T>(str);
        }


        public static T? LoadData<T>(string str)
        {
            try
            {
                T? obj = JsonConvert.DeserializeObject<T>(str, Settings);
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
