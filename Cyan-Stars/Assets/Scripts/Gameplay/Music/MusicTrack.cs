using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 音乐轨道
/// </summary>
public class MusicTrack : BaseTrack
{
    public AudioSource audioSource;
    
    /// <summary>
    /// 创建音乐轨道片段
    /// </summary>
    public static BaseClip<MusicTrack> CreateClip(MusicTrack track, int clipIndex, object userdata)
    {
        AudioClip music = (AudioClip) userdata;
        MusicClip clip = new MusicClip(0, music.length, track, music);
        return clip;
    }
}
