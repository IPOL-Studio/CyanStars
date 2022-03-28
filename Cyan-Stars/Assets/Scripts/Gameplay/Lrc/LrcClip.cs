using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// Lrc歌词片段
/// </summary>
public class LrcClip : BaseClip
{

    private string lrcText;
    
    public LrcClip(float startTime, float endTime,string lrcText) : base(startTime, endTime)
    {
        this.lrcText = lrcText;
    }

    public override void OnEnter()
    {
        //显示歌词到UI上
        GameManager.Instance.curLrcText = lrcText;
    }
}
