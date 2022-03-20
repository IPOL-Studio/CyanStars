using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CatLrcParser;

/// <summary>
/// Lrc歌词节点
/// </summary>
public class LrcNode : MusicTimeline.BaseNode
{

    private string lrcText;
    
    
    public LrcNode(float startTime,string lrcText) : base(startTime)
    {
        this.lrcText = lrcText;
    }
    
    
    public override void OnUpdate(float curTime, float deltaTime)
    {
        ShowLrcText();
        IsEnd = true;
    }

    /// <summary>
    /// 显示歌词文本
    /// </summary>
    private void ShowLrcText()
    {
        GameManager.Instance.curLrcText = lrcText;
    }


}
