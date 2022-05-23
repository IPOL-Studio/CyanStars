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
        /// 创建歌词轨道片段
        /// </summary>
        public static readonly IClipCreatorForEach<LrcTrack, LrcTimeTag> ClipCreator = new LrcClipCreator();

        private sealed class LrcClipCreator : IClipCreatorForEach<LrcTrack, LrcTimeTag>
        {
            public BaseClip<LrcTrack> CreateClip(LrcTrack track, LrcTimeTag timeTag)
            {
                float time = (float)timeTag.Timestamp.TotalSeconds;
                LrcClip clip = new LrcClip(time, time, track, timeTag.LyricText);

                return clip;
            }
        }
    }
}