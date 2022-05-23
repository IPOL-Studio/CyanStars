using System.Collections.Generic;
using CatLrcParser;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Lrc
{
    /// <summary>
    /// 歌词轨道数据
    /// </summary>
    public class LrcTrackData : ITrackData<LrcTimeTag>
    {
        public int ClipCount => ClipDataList.Count;
        
        public List<LrcTimeTag> ClipDataList { get; set; }
    }
}