using System;
using CyanStars.Chart;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Framework.Utils.JsonSerialization
{
    public sealed class ChartNoteDataReadConverter : JsonConverter<BaseChartNoteData>
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
            if (!jo.TryGetValue(nameof(BaseChartNoteData.Type), out JToken typeToken))
                throw new JsonSerializationException("Note data missing Type property");

            NoteType noteType = (NoteType)typeToken.Value<int>();

            if (!TryGetNoteData(noteType, out var noteData))
                throw new JsonSerializationException($"Unsupported note type: {noteType}");

            serializer.Populate(jo.CreateReader(), noteData);
            return noteData;
        }

        private bool TryGetNoteData(NoteType type, out BaseChartNoteData noteData)
        {
            noteData = type switch
            {
                NoteType.Tap => new TapChartNoteData(default, default),
                NoteType.Hold => new HoldChartNoteData(default, default, default, default),
                NoteType.Drag => new DragChartNoteData(default, default),
                NoteType.Click => new ClickChartNoteData(default, default),
                NoteType.Break => new BreakChartNoteData(default, default),
                _ => null
            };
            return noteData != null;
        }
    }
}
