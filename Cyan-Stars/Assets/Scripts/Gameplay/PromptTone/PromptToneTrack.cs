using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 提示音轨道
/// </summary>
public class PromptToneTrack : BaseTrack
{
    public AudioSource audioSource;
    
    /// <summary>
    /// 创建提示音轨道片段
    /// </summary>
    public static BaseClip<PromptToneTrack> CreateClip(PromptToneTrack track, int clipIndex, object userdata)
    {
        List<NoteData> noteDatas = (List<NoteData>)userdata;
        AudioClip promptTone = PromptToneHelper.Instance.GetAudioClipWithType(noteDatas[clipIndex].PromptToneType);
       
        if (promptTone == null) return new PromptToneClip(0, 0, track, promptTone);

        PromptToneClip promptToneClip = new PromptToneClip(((float)noteDatas[clipIndex].StartTime) / 1000.000f, 
            ((float)noteDatas[clipIndex].StartTime) / 1000.000f + promptTone.length, track, promptTone);
        return promptToneClip;
    }
}
