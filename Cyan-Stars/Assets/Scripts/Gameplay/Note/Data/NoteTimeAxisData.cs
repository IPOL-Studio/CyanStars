using System.Collections.Generic;
using UnityEngine;

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
        /// 速率
        /// </summary>
        [Header("速率")]
        public float SpeedRate = 1;

        /// <summary>
        /// 音符数据
        /// </summary>
        [Header("音符数据")]
        public List<NoteData> NoteDatas;
    }
}
