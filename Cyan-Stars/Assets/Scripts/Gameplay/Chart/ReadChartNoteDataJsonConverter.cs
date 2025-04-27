using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CyanStars.Gameplay.MusicGame;

namespace CyanStars.Gameplay.Chart
{
    public sealed class ReadChartNoteDataJsonConverter : JsonConverter<BaseChartNoteData>
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, BaseChartNoteData value, JsonSerializer serializer)
        {
        }

        public override BaseChartNoteData ReadJson(JsonReader reader, Type objectType, BaseChartNoteData existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            // 获取音符类型
            if (!jo.TryGetValue(nameof(BaseChartNoteData.Type), out var typeToken))
                throw new JsonSerializationException("Note data missing Type property");

            NoteType noteType = (NoteType)typeToken.Value<int>();

            // 获取对应类型
            if (!TryGetNoteDataType(noteType, out Type concreteType))
                throw new JsonSerializationException($"Unsupported note type: {noteType}");

            // 创建具体实例
            BaseChartNoteData noteData = (BaseChartNoteData)Activator.CreateInstance(concreteType);

            return noteData;
        }

        private bool TryGetNoteDataType(NoteType type, out Type concreteType)
        {
            // 类型映射字典（可根据需要改为从数据模块获取）
            var typeMapping = new Dictionary<NoteType, Type>
            {
                [NoteType.Tap] = typeof(TapChartNoteData),
                [NoteType.Hold] = typeof(HoldChartNoteData),
                [NoteType.Drag] = typeof(DragChartNoteData),
                [NoteType.Click] = typeof(ClickChartNoteData),
                [NoteType.Break] = typeof(BreakChartNoteData)
            };

            return typeMapping.TryGetValue(type, out concreteType);
        }
    }
}
