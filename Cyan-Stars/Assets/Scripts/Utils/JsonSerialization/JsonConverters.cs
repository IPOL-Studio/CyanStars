using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Utils.JsonSerialization
{
    public static class JsonConverters
    {
        /// <summary>
        /// 自定义 JsonConverter 列表
        /// </summary>
        public static readonly IList<JsonConverter> Converters = new List<JsonConverter>
        {
            new Vector2Converter(),
            new ColorConverter(),
            new ChartNoteDataReadConverter(),
            new ChartTrackDataReadConverter()
        };
    }
}
