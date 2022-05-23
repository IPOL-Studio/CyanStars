using CatLrcParser;
using CyanStars.Framework.Timeline;
using TMPro;

namespace CyanStars.Gameplay.Lrc
{
    /// <summary>
    /// Lrc歌词轨道
    /// </summary>
    public class LrcTrack : BaseTrack
    {
        public TextMeshProUGUI TxtLrc;
        
        /// <summary>
        /// 片段创建方法
        /// </summary> 
        public static readonly CreateClipFunc<LrcTrack,LrcTrackData, LrcTimeTag> CreateClipFunc = CreateClip;

        private static BaseClip<LrcTrack> CreateClip(LrcTrack track, LrcTrackData trackData, int curIndex, LrcTimeTag timeTag)
        {
            float time = (float)timeTag.Timestamp.TotalSeconds;
            LrcClip clip = new LrcClip(time, time, track, timeTag.LyricText);
            return clip;
        }



    }
}