using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Music.Timeline
{
    /// <summary>
    /// 音乐轨道
    /// </summary>
    public class MusicTrack : BaseTrack
    {
        public AudioSource audioSource;

        /// <summary>
        /// 创建音乐轨道片段
        /// </summary>
        public static readonly IClipCreator<MusicTrack, AudioClip> ClipCreator = new MusicClipCreator();

        private sealed class MusicClipCreator : IClipCreator<MusicTrack, AudioClip>
        {
            public BaseClip<MusicTrack> CreateClip(MusicTrack track, int clipIndex, AudioClip music)
            {
                return new MusicClip(0, music.length, track, music);
            }
        }
    }
}
