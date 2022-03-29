using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 音乐片段
/// </summary>
public class MusicClip : BaseClip<MusicTrack>
{
    private AudioClip music;
    
    public MusicClip(float startTime, float endTime, MusicTrack owner,AudioClip music) : base(startTime, endTime, owner)
    {
        this.music = music;
    }

    public override void OnEnter()
    {
        Owner.audioSource.clip = music;
        Owner.audioSource.Play();
    }
}
