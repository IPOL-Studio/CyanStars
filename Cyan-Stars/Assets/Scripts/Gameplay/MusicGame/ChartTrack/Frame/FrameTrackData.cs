using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameTrackData : ITrackData<FrameClipData>
    {
        public int ClipCount => ClipDataList.Count;
        public List<FrameClipData> ClipDataList { get; set; }
    }
}
