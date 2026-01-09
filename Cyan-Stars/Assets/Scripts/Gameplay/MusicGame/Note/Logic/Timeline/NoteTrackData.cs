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
        public List<BpmGroupItem> BpmGroup { get; set; }
        public List<ChartData> ClipDataList { get; set; }
    }
}
