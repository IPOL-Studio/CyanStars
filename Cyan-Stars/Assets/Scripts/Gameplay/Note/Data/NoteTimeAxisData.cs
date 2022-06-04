using System;
using System.Collections.Generic;
using CyanStars.Gameplay.Misc;
using UnityEngine;
using UnityEngine.Serialization;

namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 音符时轴数据
    /// </summary>
    [System.Serializable]
    public class NoteTimeAxisData
    {
        /// <summary>
        /// 开始时间（毫秒）
        /// </summary>
        [Header("开始时间（毫秒）")]
        public int StartTime;

        /// <summary>
        /// 缓动函数类型
        /// </summary>
        [Header("缓动函数类型")]
        public EasingFunctionType EasingType;

        /// <summary>
        /// 系数
        /// </summary>
        [Header("系数")]
        public float Coefficient = 1;

        /// <summary>
        /// 结束时间（毫秒）
        /// </summary>
        [Header("结束时间（毫秒）【不用填】")]
        public int EndTime;

        /// <summary>
        /// 视图层开始时间（毫秒）
        /// </summary>
        [Header("视图层开始时间（毫秒）【不用填】")]
        public int ViewStartTime;

        /// <summary>
        /// 视图层结束时间（毫秒）
        /// </summary>
        [Header("视图层结束时间（毫秒）【不用填】")]
        public int ViewEndTime;

        /// <summary>
        /// 音符数据
        /// </summary>
        [Header("音符数据")]
        public List<NoteData> NoteDatas;
    }
}
