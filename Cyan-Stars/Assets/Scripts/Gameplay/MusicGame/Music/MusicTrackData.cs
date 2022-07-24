using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class MusicTrackData : ITrackData<AudioClip>
    {
        public int ClipCount => 1;
        public List<AudioClip> ClipDataList { get; set; }
    }
}
