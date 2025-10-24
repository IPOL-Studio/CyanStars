using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CyanStars.Framework.Utils.JsonSerialization
{
    public class Vector2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // 能够处理 Vector2 和 可空的 Vector2? (Nullable<Vector2>)
            return objectType == typeof(Vector2) || objectType == typeof(Vector2?);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                // 如果是 null (对应 Vector2? 的 null 值)，就写入 null
                writer.WriteNull();
                return;
            }

            // 将 object 转换为 Vector2
            // 无论是 Vector2 还是 Vector2?，都可以直接转换
            Vector2 vector = (Vector2)value;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(vector.x);
            writer.WritePropertyName("y");
            writer.WriteValue(vector.y);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // 如果 JSON 中的值是 null
            if (reader.TokenType == JsonToken.Null)
            {
                // 如果目标类型允许为 null (即 Vector2?)，则返回 null
                if (objectType == typeof(Vector2?))
                {
                    return null;
                }

                // 如果目标类型不允许为 null (即 Vector2)，则返回默认值
                return Vector2.zero;
            }

            JObject obj = JObject.Load(reader);
            float x = (float)obj["x"];
            float y = (float)obj["y"];
            return new Vector2(x, y);
        }
    }
}
