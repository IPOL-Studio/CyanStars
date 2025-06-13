using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class MusicTrackData : ITrackData<MusicClipData>
    {
        public int ClipCount => 1;
        public List<MusicClipData> ClipDataList { get; set; }
    }
}
