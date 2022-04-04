using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.Effect.Timeline
{
    /// <summary>
    /// 特效轨道
    /// </summary>
    public class EffectTrack : BaseTrack
    {
        public float Bpm;
        public List<GameObject> EffectGOs;
        public Transform EffectParent;
        public Image Frame;

        //因为边框呼吸特效的clip持续时间覆盖到了粒子特效的clip，如果想正确播放粒子特效需要使用All模式来处理
        protected override ClipProcessMode Mode => ClipProcessMode.All;

        /// <summary>
        /// 创建特效轨道片段
        /// </summary>
        public static readonly IClipCreator<EffectTrack, IList<EffectControllerSo.KeyFrame>> ClipCreator =
            new EffectClipCreator();

        private sealed class EffectClipCreator : IClipCreator<EffectTrack, IList<EffectControllerSo.KeyFrame>>
        {
            public BaseClip<EffectTrack> CreateClip(EffectTrack track, int clipIndex, IList<EffectControllerSo.KeyFrame> keyFrames)
            {
                EffectControllerSo.KeyFrame keyFrame = keyFrames[clipIndex];

                float time = keyFrame.time / 1000f;
                float duration = keyFrame.duration / 1000f;

                BaseClip<EffectTrack> clip = null;
                switch (keyFrame.type)
                {
                    case EffectType.FrameBreath:
                        clip = new FrameBreathClip(time, time + duration, track, duration, keyFrame.color,
                            keyFrame.intensity,
                            keyFrame.maxAlpha, keyFrame.minAlpha);
                        break;

                    case EffectType.FrameOnce:
                        Debug.LogError("EffectType.FrameOnce 没实现捏");
                        break;

                    case EffectType.Particle:
                        clip = new ParticleEffectClip(time, time, track, keyFrame.index, keyFrame.position,
                            keyFrame.rotation,
                            keyFrame.particleCount, duration);
                        break;
                }

                return clip;
            }
        }
    }
}