using System;
using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面轨道数据拓展格式
    /// </summary>
    [Serializable]
    public class ChartTrackData
    {
        /// <summary>
        /// 轨道名
        /// </summary>
        public string TrackName;

        /// <summary>
        /// 轨道数据，需要在创建轨道时处理
        /// </summary>
        public List<ChartTrackEventData> ChartTrackEventDatas;
    }

    /// <summary>
    /// 轨道事件数据
    /// </summary>
    /// <remarks>可被条件控制的事件</remarks>
    [Serializable]
    public class ChartTrackEventData
    {
        /// <summary>
        /// 引用的条件的索引，满足条件时才会执行事件
        /// </summary>
        public int ConditionIndex;

        /// <summary>
        /// 事件
        /// </summary>
        public Dictionary<string, object> EventData;
    }
}
