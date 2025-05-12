// using UnityEngine;
// using CyanStars.Framework.Timeline;
//
// namespace CyanStars.Gameplay.MusicGame
// {
//     /// <summary>
//     /// 边框单次闪烁特效片段
//     /// </summary>
//     public class FrameOnceClip : BaseClip<EffectTrack>
//     {
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
//         /// 播放次数
//         /// </summary>
//         private int frequency;
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
//         public FrameOnceClip(float startTime, float endTime, EffectTrack owner, Color color,
//             float intensity, int frequency, float maxAlpha, float minAlpha, float bpm) : base(startTime, endTime, owner)
//         {
//             this.color = color;
//             this.intensity = intensity;
//             this.frequency = frequency;
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
//             float t = (currentTime - StartTime) % (60 / bpm);
//             float alpha = EasingFunction.EaseOutQuart(maxAlpha, minAlpha, t, 60 / bpm);
//             color.a = alpha;
//             Owner.ImgFrame.color = color;
//         }
//     }
// }
