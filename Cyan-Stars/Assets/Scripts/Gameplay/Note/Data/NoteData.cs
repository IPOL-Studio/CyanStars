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
    public const float MaxPos = 1;
    public const float MiddlePos = MaxPos / 2;
    
    /// <summary>
    /// 音符类型
    /// </summary>
    public NoteType Type;

    /// <summary>
    /// 位置坐标
    /// 处于中间的位置范围在[0-1]，Break音符则左-1右2
    /// </summary>
    [Range(-1,2)]
    public float Pos;

    /// <summary>
    /// 音符宽度
    /// 该值不变，恒为0.2
    /// </summary>
    //[Range(0,1)]
    [Header("不要改变这个值，这个值不变，恒为0.2")]
    public float Width= 0.2f;

    /// <summary>
    /// 判定开始时间（毫秒）
    /// </summary>
    public int StartTime;
    
    /// <summary>
    /// Hold音符的判定结束时间（毫秒）
    /// </summary>
    public int HoldEndTime;

    /// <summary>
    /// 提示音
    /// </summary>
    public PromptToneType PromptToneType = PromptToneType.ns_ka;
    
    public int GetFullScore()
    {
        if (Type == NoteType.Hold || Type == NoteType.Break || Type == NoteType.Click)
        {
            return 2;
        }
        else 
        {
            return 1;
        }
    }

    
    public override string ToString()
    {
        return $"音符数据：类型{Type}，位置{Pos}，宽度{Width},开始时间{StartTime},Hold音符结束时间{HoldEndTime}";
    }
}
