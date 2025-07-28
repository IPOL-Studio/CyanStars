namespace CyanStars.Chart
{
    /// <summary>
    /// 谱面轨道数据拓展格式
    /// </summary>
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
