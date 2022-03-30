using System.Collections;
using System.Collections.Generic;
using CatLrcParser;
using CatTimeline;

/// <summary>
/// Lrc歌词轨道
/// </summary>
public class LrcTrack : BaseTrack
{

    /// <summary>
    /// 创建歌词轨道片段
    /// </summary>
    public static BaseClip<LrcTrack> CreateClip(LrcTrack track, int clipIndex, object userdata)
    {
        List<LrcTimeTag> timeTags = (List<LrcTimeTag>) userdata;
        LrcTimeTag timeTag = timeTags[clipIndex];

        float time = (float) timeTag.Timestamp.TotalSeconds;
        LrcClip clip = new LrcClip(time, time,track, timeTag.LyricText);
            
        return clip;
    }
}
