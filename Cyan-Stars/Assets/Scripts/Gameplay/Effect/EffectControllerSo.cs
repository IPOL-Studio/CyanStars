using System.Collections;
using System.Collections.Generic;
using CyanStars.Gameplay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Effect
{
    // public enum EffectType
    // {
    //     FrameBreath,
    //     FrameOnce,
    //     Particle,
    // }

    public class EffectControllerSo : MonoBehaviour
    {

        [Header("特效种类")] public List<GameObject> effectList;

        [System.Serializable]
        public class KeyFrame
        {
            [Header("种类")] public EffectType type;
            [Header("时间")] public float time;
            [Header("特效序号(仅对particle有效)")] public int index;
            [Header("位置(仅对particle有效)")] public Vector3 position;
            [Header("朝向(仅对particle有效)")] public Vector3 rotation;

            [Header("粒子数量(仅对particle有效，此处若小于等于0则使用默认值)")]
            public int particleCount;

            [Header("持续时间(仅对particle和frame.breath有效)")]
            public float duration;

            [Header("颜色(仅对frame有效)")] public Color color;

            [Header("强度(仅对frame有效)")] [Range(0, 1)]
            public float intensity;

            [Header("播放次数(仅对frame.once有效)")] public int frequency;

            [Header("最大透明度(仅对frame有效)")] [Range(0, 1)]
            public float maxAlpha;

            [Header("最小透明度(仅对frame有效)")] [Range(0, 1)]
            public float minAlpha;
        }


        [Header("边框")] public Image frame;

        [Header("BPM")] public float bpm;

        [Header("关键帧")] public List<KeyFrame> keyFrames;



        IEnumerator FrameFade(int frequency, float maxAlpha, float minAlpha)
        {
            Color color = frame.color;
            for (int i = 0; i < frequency; i++)
            {
                for (float t = 0; t < 60 / bpm; t += Time.deltaTime)
                {
                    color.a = SmoothFuncation.CubicFuncation(minAlpha, maxAlpha, t, 60 / bpm);
                    frame.color = color;
                    yield return null;
                }
            }
        }

    }
}