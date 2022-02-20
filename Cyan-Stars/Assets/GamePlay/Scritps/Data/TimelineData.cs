using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴数据
/// </summary>
[Serializable]
public class TimelineData
{
    /// <summary>
    /// 总时间
    /// </summary>
    public float Time;

    /// <summary>
    /// 速率
    /// </summary>
    public float SpeedRate = 1;
    
    /// <summary>
    /// 轨道数据列表
    /// </summary>
    public List<TrackData> TrackDatas;
}
