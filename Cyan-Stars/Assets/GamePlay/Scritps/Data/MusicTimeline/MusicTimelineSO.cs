using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 音乐时间轴数据的SO
/// </summary>
[CreateAssetMenu(menuName = "音乐时间轴配置")]
public class MusicTimelineSO : ScriptableObject
{
    public MusicTimelineData musicTimelineData;
}
