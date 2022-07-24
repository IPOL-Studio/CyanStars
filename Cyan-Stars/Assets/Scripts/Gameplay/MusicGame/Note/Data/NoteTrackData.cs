using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符轨道数据
    /// </summary>
    [System.Serializable]
    public class NoteTrackData : ITrackData<NoteLayerData>
    {
        /// <summary>
        /// 音符基础速度
        /// </summary>
        [Header("音符基础速度")]
        public float BaseSpeed = 1;

        /// <summary>
        /// 谱面整体速率
        /// </summary>
        [Header("谱面整体速率")]
        public float SpeedRate = 1;

        /// <summary>
        /// 音符图层数据
        /// </summary>
        [Header("音符图层数据")]
        public List<NoteLayerData> LayerDatas;

        public int ClipCount => 1;

        public List<NoteLayerData> ClipDataList => LayerDatas;
    }
}
