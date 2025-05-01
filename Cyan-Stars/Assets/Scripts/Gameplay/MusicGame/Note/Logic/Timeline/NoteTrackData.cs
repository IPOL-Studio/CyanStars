using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符轨道数据
    /// </summary>
    [System.Serializable]
    public class NoteTrackData : ITrackData<ChartData>
    {
        public int ClipCount => 1;

        public List<ChartData> ClipDataList { get; set; }
    }
}
