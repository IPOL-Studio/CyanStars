// TODO: 暂时停用，待重构
// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using CyanStars.Framework.Timeline;
//
// namespace CyanStars.Gameplay.MusicGame
// {
//     /// <summary>
//     /// 特效轨道
//     /// </summary>
//     public class EffectTrack : BaseTrack
//     {
//         public List<string> EffectNames;
//         public Transform EffectParent;
//         public Image ImgFrame;
//
//         /// <summary>
//         /// 片段创建方法
//         /// </summary>
//         public static readonly CreateClipFunc<EffectTrack, EffectTrackData, EffectTrackData.KeyFrame> CreateClipFunc = CreateClip;
//
//         private static BaseClip<EffectTrack> CreateClip(EffectTrack track, EffectTrackData trackData, int curIndex, EffectTrackData.KeyFrame keyFrame)
//         {
//             float time = keyFrame.Time / 1000f;
//             float duration = keyFrame.Duration / 1000f;
//             float bpm = keyFrame.BPM;
//             int frequency = keyFrame.Frequency;
//
//             BaseClip<EffectTrack> clip = null;
//             switch (keyFrame.Type)
//             {
//                 case EffectType.FrameBreath:
//                     clip = new FrameBreathClip(time, time + duration, track, duration, keyFrame.Color,
//                         keyFrame.Intensity,
//                         keyFrame.MaxAlpha, keyFrame.MinAlpha,
//                         keyFrame.BPM);
//                     break;
//
//                 case EffectType.FrameOnce:
//                     clip = new FrameOnceClip(time, time + 60 / bpm * frequency, track, keyFrame.Color,
//                         keyFrame.Intensity,keyFrame.Frequency,
//                         keyFrame.MaxAlpha, keyFrame.MinAlpha,
//                         keyFrame.BPM);
//                     break;
//
//                 case EffectType.Particle:
//                     clip = new ParticleEffectClip(time, time, track, keyFrame.Index, keyFrame.Position,
//                         keyFrame.Rotation,
//                         keyFrame.ParticleCount, duration);
//                     break;
//             }
//
//             return clip;
//         }
//     }
// }
