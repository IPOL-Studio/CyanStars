using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音乐时间轴数据
/// </summary>
[System.Serializable]
public class MusicTimelineData
{
    /// <summary>
    /// 总时间
    /// </summary>
    public float Time;

    /// <summary>
    /// 速度
    /// </summary>
    public float Speed = 1;
    
    /// <summary>
    /// 速率
    /// </summary>
    public float SpeedRate = 1;
    
    /// <summary>
    /// 图层数据
    /// </summary>
    public List<LayerData> LayerDatas;
}
