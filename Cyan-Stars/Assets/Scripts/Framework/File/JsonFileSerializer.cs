#nullable enable

using System;
using System.Globalization;
using CyanStars.Utils.JsonSerialization;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// Json 序列化工具
    /// </summary>
    /// <remarks>
    /// <para>只负责对象 ↔ 字符串，落盘由调用方直接覆盖写入目标文件。</para>
    /// </remarks>
    public static class JsonFileSerializer
    {
        /// <summary>
        /// 序列化格式参数
        /// </summary>
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented,
            Culture = CultureInfo.InvariantCulture,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Converters = JsonConverters.Converters
        };


        /// <summary>
        /// 序列化对象为 json 字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="json">序列化结果，失败时为空字符串</param>
        /// <returns>是否成功序列化</returns>
        public static bool TrySerialize(object obj, out string json)
        {
            try
            {
                json = JsonConvert.SerializeObject(obj, Settings);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化时出现异常：{e}");
                json = "";
                return false;
            }
        }
    }
}
