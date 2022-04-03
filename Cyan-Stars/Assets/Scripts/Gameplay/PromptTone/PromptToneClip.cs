using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 提示音片段
/// </summary>
public class PromptToneClip : BaseClip<PromptToneTrack>
{
    private AudioClip promptTone;

    class NoteStartTimeCompare : IComparer<NoteData>
    {
        public int Compare(NoteData x, NoteData y)
        {
            return x.StartTime.CompareTo(y.StartTime);
        }
    }

    public PromptToneClip(float startTime, float endTime, PromptToneTrack owner,
        AudioClip promptTone) : base(startTime, endTime, owner)
    {
        this.promptTone = promptTone;
    }

    public override void OnEnter()
    {
        Owner.audioSource.PlayOneShot(promptTone);
    }
}
