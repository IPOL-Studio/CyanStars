using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;

namespace CyanStars.Gameplay.Effect
{
    /// <summary>
    /// 特效轨道
    /// </summary>
    public class EffectTrack : BaseTrack
    {
        public float BPM;
        public List<GameObject> EffectPrefabs;
        public Transform EffectParent;
        public Image Frame;

        //因为边框呼吸特效的clip持续时间覆盖到了粒子特效的clip，如果想正确播放粒子特效需要使用All模式来处理
        protected override ClipProcessMode Mode => ClipProcessMode.All;

        /// <summary>
        /// 创建特效轨道片段
        /// </summary>
        public static readonly IClipCreator<EffectTrack, EffectTrackData> ClipCreator =
            new EffectClipCreator();

        private sealed class EffectClipCreator : IClipCreator<EffectTrack, EffectTrackData>
        {
            public BaseClip<EffectTrack> CreateClip(EffectTrack track, int clipIndex, EffectTrackData data)
            {
                EffectTrackData.KeyFrame keyFrame = data.KeyFrames[clipIndex];

                float time = keyFrame.Time / 1000f;
                float duration = keyFrame.Duration / 1000f;

                BaseClip<EffectTrack> clip = null;
                switch (keyFrame.Type)
                {
                    case EffectType.FrameBreath:
                        clip = new FrameBreathClip(time, time + duration, track, duration, keyFrame.Color,
                            keyFrame.Intensity,
                            keyFrame.MaxAlpha, keyFrame.MinAlpha);
                        break;

                    case EffectType.FrameOnce:
                        Debug.LogError("EffectType.FrameOnce 没实现捏");
                        break;

                    case EffectType.Particle:
                        clip = new ParticleEffectClip(time, time, track, keyFrame.Index, keyFrame.Position,
                            keyFrame.Rotation,
                            keyFrame.ParticleCount, duration);
                        break;
                }

                return clip;
            }
        }
    }
}