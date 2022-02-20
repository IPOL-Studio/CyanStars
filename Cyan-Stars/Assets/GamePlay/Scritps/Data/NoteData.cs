using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音符数据
/// </summary>
[Serializable]
public class NoteData
{
    /// <summary>
    /// 音符类型
    /// </summary>
    public NoteType Type;
    
    /// <summary>
    /// 时间点
    /// </summary>
    public float TimePoint;
    
    
    /// <summary>
    /// 速率
    /// </summary>
    public float SpeedRate = 1;

    /// <summary>
    /// Hold音符的长度
    /// </summary>
    public float HoldLength = 1;
}
