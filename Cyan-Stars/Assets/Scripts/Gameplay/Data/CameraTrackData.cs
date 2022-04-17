using System.Collections;
using System.Collections.Generic;
using CyanStars.Gameplay;
using UnityEngine;

namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 相机轨道数据
    /// </summary>
    [System.Serializable]
    public class CameraTrackData
    {
        [Header("相机默认位置")]
        public Vector3 DefaultPosition;
        
        [Header("相机默认角度")]
        public Vector3 DefaultRotation;

        [System.Serializable]
        public class KeyFrame
        {
            [Header("时间")] public float Time;
            [Header("目标")] public Vector3 Position;
            public Vector3 Rotation;

            [Header("缓动方式")] public SmoothFuncationType SmoothType;
        }

        [Header("关键帧")] public List<KeyFrame> KeyFrames;
    }
}


