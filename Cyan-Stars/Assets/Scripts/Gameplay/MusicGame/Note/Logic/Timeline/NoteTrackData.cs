using System;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符轨道数据
    /// </summary>
    [Serializable]
    public class NoteTrackData : ITrackData<ChartData>
    {
        public int ClipCount => 1;
        public ChartContext ChartContext { get; init; }
        public List<ChartData> ClipDataList { get; init; }
    }
}
