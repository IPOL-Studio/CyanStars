using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音符数据
/// </summary>
[System.Serializable]
public class NoteData
{
    /// <summary>
    /// 音符类型
    /// </summary>
    public NoteType Type;

    /// <summary>
    /// 位置坐标
    /// 处于中间的位置范围在[0-1]，Break音符则左-1右2
    /// </summary>
    public float Pos;

    /// <summary>
    /// 音符宽度
    /// 用于判定能被哪些input接住，范围[0-1],1表示铺满中间
    /// Break音符无视此值
    /// </summary>
    [Range(0,1)]
    public float Width;

    /// <summary>
    /// 判定开始时间
    /// </summary>
    public float StartTime;
    
    /// <summary>
    /// Hold音符的判定结束时间
    /// </summary>
    public float HoldEndTime;
    
    
    public override string ToString()
    {
        return $"音符数据：类型{Type}，位置{Pos}，宽度{Width},开始时间{StartTime},Hold音符长度{HoldEndTime}";
    }
}
