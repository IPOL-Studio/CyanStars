using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴片段数据
/// </summary>
[System.Serializable]
public class ClipData
{
    /// <summary>
    /// 开始时间（毫秒）
    /// </summary>
    public int StartTime;
    
    /// <summary>
    /// 速率
    /// </summary>
    public float SpeedRate = 1;
    
    /// <summary>
    /// 音符数据
    /// </summary>
    public List<NoteData> NoteDatas;
}
