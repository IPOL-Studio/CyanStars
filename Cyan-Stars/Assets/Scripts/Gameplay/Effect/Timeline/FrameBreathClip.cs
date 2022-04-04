using UnityEngine;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.Effect.Timeline
{
    /// <summary>
    /// 边框呼吸特效片段
    /// </summary>
    public class FrameBreathClip : BaseClip<EffectTrack>
    {
        /// <summary>
        /// 持续时间
        /// </summary>
        private float duration;

        /// <summary>
        /// 颜色
        /// </summary>
        private Color color;

        /// <summary>
        /// 强度
        /// </summary>
        private float intensity;

        /// <summary>
        /// 最大透明度
        /// </summary>
        private float maxAlpha;

        /// <summary>
        /// 最小透明度
        /// </summary>
        private float minAlpha;

        public FrameBreathClip(float startTime, float endTime, EffectTrack owner, float duration, Color color,
            float intensity, float maxAlpha, float minAlpha) : base(startTime, endTime, owner)
        {
            this.duration = duration;
            this.color = color;
            this.intensity = intensity;
            this.maxAlpha = maxAlpha;
            this.minAlpha = minAlpha;
        }

        public override void OnEnter()
        {
            Owner.Frame.color = color;
            Owner.Frame.pixelsPerUnitMultiplier = 1 - intensity;
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            float bpm = Owner.Bpm;
            float alpha = (0.5f * Mathf.Cos(bpm * 2 * Mathf.PI * (currentTime - StartTime - 30 / bpm) / 60) + 0.5f) *
                (maxAlpha - minAlpha) + minAlpha;

            color.a = alpha;
            Owner.Frame.color = color;
        }
    }
}
