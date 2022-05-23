using System;
using UnityEngine;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Camera;
using CyanStars.Gameplay.Effect;

namespace CyanStars.Gameplay.MapData
{
    /// <summary>
    /// 谱面时间轴数据
    /// </summary>
    [Serializable]
    public class MapTimelineData
    {
        [NonSerialized]
        public int Time;

        /// <summary>
        /// 音符轨道数据
        /// </summary>
        [Header("音符轨道数据")]
        public NoteTrackData NoteTrackData;

        /// <summary>
        /// 相机轨道数据
        /// </summary>
        [Header("相机轨道数据")]
        public CameraTrackData CameraTrackData;

        /// <summary>
        /// 特效轨道数据
        /// </summary>
        [Header("特效轨道数据")]
        public EffectTrackData EffectTrackData;
    }
}
