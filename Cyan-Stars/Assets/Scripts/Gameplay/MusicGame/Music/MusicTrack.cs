using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音乐轨道
    /// </summary>
    public class MusicTrack : BaseTrack
    {
        public AudioSource AudioSource;

        /// <summary>
        /// 片段创建方法
        /// </summary>
        public static readonly CreateClipFunc<MusicTrack, MusicTrackData, MusicClipData> CreateClipFunc = CreateClip;

        private static BaseClip<MusicTrack> CreateClip(MusicTrack track, MusicTrackData trackData, int curIndex,
            MusicClipData musicClipData)
        {
            float clipEndTime = musicClipData.audioClip.length + musicClipData.offset / 1000f;
            float clipStartTime;
            float prePlayTime;
            if (musicClipData.offset < 0)
            {
                clipStartTime = 0;
                prePlayTime = -musicClipData.offset / 1000f;
            }
            else
            {
                clipStartTime = musicClipData.offset / 1000f;
                prePlayTime = 0;
            }

            return new MusicClip(clipStartTime, clipEndTime, prePlayTime, track, musicClipData.audioClip);
        }
    }
}
