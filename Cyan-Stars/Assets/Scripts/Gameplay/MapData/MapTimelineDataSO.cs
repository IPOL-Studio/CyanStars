using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MapData
{
    /// <summary>
    /// 谱面时间轴数据SO
    /// </summary>
    [CreateAssetMenu(menuName = "创建谱面时间轴数据SO文件")]
    public class MapTimelineDataSO : ScriptableObject
    {
        [Header("谱面时间轴数据")]
        public MapTimelineData Data;
    }
}

