using System;
using System.Collections.Generic;
using CyanStars.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanStars.Gameplay.Chart
{
    public sealed class ChartTrackDataReadConverter : JsonConverter<ChartTrackData>
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, ChartTrackData value, JsonSerializer serializer)
        {
        }

        public override ChartTrackData ReadJson(JsonReader reader, Type objectType, ChartTrackData existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (!jo.TryGetValue(nameof(ChartTrackData.TrackKey), out JToken keyToken))
                throw new JsonSerializationException(
                    $"A track data missing {nameof(ChartTrackData.TrackKey)} property in json.");

            string trackKey = keyToken.Value<string>();
            if (!TryGetChartTrackType(trackKey, out Type trackType))
                throw new KeyNotFoundException($"A track data with key {trackKey} not found in chart track type list.");

            if (!jo.TryGetValue(nameof(ChartTrackData.TrackData), out JToken trackToken))
                return new ChartTrackData(trackKey, null);

            if (trackToken.Type != JTokenType.Object)
                throw new JsonSerializationException($"A track data with key {trackKey} is not a valid json object.");

            IChartTrackData track = (IChartTrackData)serializer.Deserialize(trackToken.CreateReader(), trackType);
            return new ChartTrackData(trackKey, track);
        }

        private bool TryGetChartTrackType(string key, out Type type)
        {
            type = null;
            ChartDataModule dataModule = GameRoot.GetDataModule<ChartDataModule>();

            return dataModule is { } && dataModule.TryGetChartTrackType(key, out type);
        }
    }
}
