using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Chart
{
    /// <summary>
    /// 谱面轨道数据拓展格式
    /// </summary>
    [JsonConverter(typeof(ReadChartTrackDataJsonConverter))]
    public sealed class ChartTrackData
    {
        public string TrackKey { get; }
        public IChartTrackData TrackData { get; }

        public ChartTrackData(string trackKey, IChartTrackData trackData)
        {
            TrackKey = trackKey;
            TrackData = trackData;
        }
    }
}
