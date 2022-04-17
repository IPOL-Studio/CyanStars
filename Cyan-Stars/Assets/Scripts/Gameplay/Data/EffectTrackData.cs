using System.Collections;
using System.Collections.Generic;
using CyanStars.Gameplay.Effect;
using UnityEngine;

namespace CyanStars.Gameplay.Data
{
    public enum EffectType
    {
        FrameBreath,
        FrameOnce,
        Particle,
    }
    
    /// <summary>
    /// 特效轨道数据
    /// </summary>
    [System.Serializable]
    public class EffectTrackData
    {
        

        [System.Serializable]
        public class KeyFrame
        {
            [Header("种类")] public EffectType Type;
            [Header("时间")] public float Time;
            [Header("特效序号(仅对particle有效)")] public int Index;
            [Header("位置(仅对particle有效)")] public Vector3 Position;
            [Header("朝向(仅对particle有效)")] public Vector3 Rotation;

            [Header("粒子数量(仅对particle有效，此处若小于等于0则使用默认值)")]
            public int ParticleCount;

            [Header("持续时间(仅对particle和frame.breath有效)")]
            public float Duration;

            [Header("颜色(仅对frame有效)")] public Color Color;

            [Header("强度(仅对frame有效)")] [Range(0, 1)]
            public float Intensity;

            [Header("播放次数(仅对frame.once有效)")] public int Frequency;

            [Header("最大透明度(仅对frame有效)")] [Range(0, 1)]
            public float MaxAlpha;

            [Header("最小透明度(仅对frame有效)")] [Range(0, 1)]
            public float MinAlpha;
        }

        [Header("BPM")] public float BPM;

        [Header("关键帧")] public List<KeyFrame> KeyFrames;
    }
}


