using System;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameClip : BaseClip<FrameTrack>
    {
        private FrameType type;
        private Color color;
        private float intensity;
        private float bpm;
        private float minAlpha;
        private float maxAlpha;


        public FrameClip(float startTime, float endTime, FrameTrack owner, FrameType type, Color color, float intensity,
            float bpm, float minAlpha, float maxAlpha) : base(startTime, endTime, owner)
        {
            this.type = type;
            this.color = color;
            this.intensity = intensity;
            this.bpm = bpm;
            this.minAlpha = minAlpha;
            this.maxAlpha = maxAlpha;
        }

        public override void OnEnter()
        {
            Owner.ImgFrame.color = color;
            Owner.ImgFrame.pixelsPerUnitMultiplier = 1 - intensity;
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            switch (type)
            {
                case FrameType.Flash:
                {
                    float t = (currentTime - StartTime) % (60 / bpm);
                    float alpha = EasingFunction.EaseOutQuart(maxAlpha, minAlpha, t, 60 / bpm);
                    color.a = alpha;
                    Owner.ImgFrame.color = color;
                    break;
                }
                case FrameType.Breath:
                {
                    float alpha = Mathf.Abs(Mathf.Sin((currentTime - StartTime) * bpm * Mathf.PI / 60)) *
                        (maxAlpha - minAlpha) + minAlpha;
                    color.a = alpha;
                    Owner.ImgFrame.color = color;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
