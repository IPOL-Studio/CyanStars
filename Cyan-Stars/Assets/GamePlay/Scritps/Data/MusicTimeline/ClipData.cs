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
    /// 开始时间
    /// </summary>
    public float StartTime;
    
    /// <summary>
    /// 速率
    /// </summary>
    public float SpeedRate = 1;
}
