using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Music
{
    /// <summary>
    /// 音乐轨道
    /// </summary>
    public class MusicTrack : BaseTrack
    {
        public AudioSource audioSource;

        /// <summary>
        /// 片段创建方法
        /// </summary> 
        public static readonly CreateClipFunc<MusicTrack,MusicTrackData, AudioClip> CreateClipFunc = CreateClip;

        private static BaseClip<MusicTrack> CreateClip(MusicTrack track, MusicTrackData trackData, int curIndex, AudioClip music)
        {
            return new MusicClip(0, music.length, track, music);
        }


        

    }
}
