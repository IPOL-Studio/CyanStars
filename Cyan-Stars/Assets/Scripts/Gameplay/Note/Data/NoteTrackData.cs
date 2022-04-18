using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 音符轨道数据
    /// </summary>
    [System.Serializable]
    public class NoteTrackData
    {
        /// <summary>
        /// 音符基础速度
        /// </summary>
        [Header("音符基础速度")]
        public float BaseSpeed = 1;

        /// <summary>
        /// 音符速率
        /// </summary>
        [Header("音符速率")]
        public float SpeedRate = 1;

        /// <summary>
        /// 音符图层数据
        /// </summary>
        [Header("音符图层数据")]
        public List<NoteLayerData> LayerDatas;
    }
}


