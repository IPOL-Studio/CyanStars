using System.Collections.Generic;
using UnityEngine;
using CyanStars.Framework.Timeline;

using UnityEngine.Serialization;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 相机轨道数据
    /// </summary>
    [System.Serializable]
    public class CameraTrackData : ITrackData<CameraTrackData.KeyFrame>
    {
        [Header("相机默认位置")]
        public Vector3 DefaultPosition;

        [Header("相机默认角度")]
        public Vector3 DefaultRotation;

        [System.Serializable]
        public class KeyFrame
        {
            [Header("时间")]
            public float Time;

            [Header("目标")]
            public Vector3 Position;
            public Vector3 Rotation;

            [Header("缓动方式")]
            public EasingFunctionType EasingType;
        }

        [Header("关键帧")]
        public List<KeyFrame> KeyFrames;

        public int ClipCount => KeyFrames.Count;
        public List<KeyFrame> ClipDataList => KeyFrames;
    }
}
