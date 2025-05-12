// TODO: Refactor this file

// using UnityEngine;
// using CyanStars.Framework.Timeline;
//
// namespace CyanStars.Gameplay.MusicGame
// {
//     /// <summary>
//     /// 提示音片段
//     /// </summary>
//     public class PromptToneClip : BaseClip<PromptToneTrack>
//     {
//         private AudioClip promptTone;
//
//         public PromptToneClip(float startTime, float endTime, PromptToneTrack owner, AudioClip promptTone) :
//             base(startTime, endTime, owner)
//         {
//             this.promptTone = promptTone;
//         }
//
//         public override void OnEnter()
//         {
//             Owner.AudioSource.PlayOneShot(promptTone);
//         }
//     }
// }
