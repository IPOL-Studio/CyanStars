using CatLrcParser;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Lrc
{
    /// <summary>
    /// Lrc歌词轨道
    /// </summary>
    public class LrcTrack : BaseTrack
    {
        /// <summary>
        /// 创建歌词轨道片段
        /// </summary>
        public static readonly IClipCreator<LrcTrack, IList<LrcTimeTag>> ClipCreator = new LrcClipCreator();

        private sealed class LrcClipCreator : IClipCreator<LrcTrack, IList<LrcTimeTag>>
        {
            public BaseClip<LrcTrack> CreateClip(LrcTrack track, int clipIndex, IList<LrcTimeTag> timeTags)
            {
                LrcTimeTag timeTag = timeTags[clipIndex];

                float time = (float)timeTag.Timestamp.TotalSeconds;
                LrcClip clip = new LrcClip(time, time, track, timeTag.LyricText);

                return clip;
            }
        }
    }
}