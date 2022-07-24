using System;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面时间轴数据
    /// </summary>
    [Serializable]
    public class MapTimelineData
    {
        /// <summary>
        /// 时间轴长度（毫秒）
        /// </summary>
        [Header("时间轴长度（毫秒）")]
        public int Length;

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
