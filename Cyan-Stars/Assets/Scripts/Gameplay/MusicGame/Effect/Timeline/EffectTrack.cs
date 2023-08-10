using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 特效轨道
    /// </summary>
    public class EffectTrack : BaseTrack
    {
        public List<string> EffectNames;
        public Transform EffectParent;
        public Image ImgFrame;

        /// <summary>
        /// 片段创建方法
        /// </summary>
        public static readonly CreateClipFunc<EffectTrack, EffectTrackData, EffectTrackData.KeyFrame> CreateClipFunc = CreateClip;

        private static BaseClip<EffectTrack> CreateClip(EffectTrack track, EffectTrackData trackData, int curIndex, EffectTrackData.KeyFrame keyFrame)
        {
            float time = keyFrame.Time / 1000f;
            float duration = keyFrame.Duration / 1000f;

            BaseClip<EffectTrack> clip = null;
            switch (keyFrame.Type)
            {
                case EffectType.FrameBreath:
                    clip = new FrameBreathClip(time, time + duration, track, duration, keyFrame.Color,
                        keyFrame.Intensity,
                        keyFrame.MaxAlpha, keyFrame.MinAlpha,
                        keyFrame.BPM);
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
