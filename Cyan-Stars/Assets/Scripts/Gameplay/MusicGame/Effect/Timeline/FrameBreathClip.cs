// TODO: 暂时停用，待重构
// using UnityEngine;
// using CyanStars.Framework.Timeline;
//
// namespace CyanStars.Gameplay.MusicGame
// {
//     /// <summary>
//     /// 边框呼吸特效片段
//     /// </summary>
//     public class FrameBreathClip : BaseClip<EffectTrack>
//     {
//         /// <summary>
//         /// 持续时间
//         /// </summary>
//         private float duration;
//
//         /// <summary>
//         /// 颜色
//         /// </summary>
//         private Color color;
//
//         /// <summary>
//         /// 强度
//         /// </summary>
//         private float intensity;
//
//         /// <summary>
//         /// 最大透明度
//         /// </summary>
//         private float maxAlpha;
//
//         /// <summary>
//         /// 最小透明度
//         /// </summary>
//         private float minAlpha;
//
//         /// <summary>
//         /// bpm
//         /// </summary>
//         private float bpm;
//
//         public FrameBreathClip(float startTime, float endTime, EffectTrack owner, float duration, Color color,
//             float intensity, float maxAlpha, float minAlpha, float bpm) : base(startTime, endTime, owner)
//         {
//             this.duration = duration;
//             this.color = color;
//             this.intensity = intensity;
//             this.maxAlpha = maxAlpha;
//             this.minAlpha = minAlpha;
//             this.bpm = bpm;
//         }
//
//         public override void OnEnter()
//         {
//             Owner.ImgFrame.color = color;
//             Owner.ImgFrame.pixelsPerUnitMultiplier = 1 - intensity;
//         }
//
//         public override void OnUpdate(float currentTime, float previousTime)
//         {
//             float alpha = Mathf.Abs(Mathf.Sin((currentTime - StartTime) * bpm * Mathf.PI / 60)) * (maxAlpha - minAlpha) + minAlpha;
//             color.a = alpha;
//             Owner.ImgFrame.color = color;
//         }
//     }
// }
