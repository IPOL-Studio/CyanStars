using CyanStars.Framework.Timeline;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 提示音片段
    /// </summary>
    public class PromptToneClip : BaseKeyClip<PromptToneTrack>
    {
        public AudioSource AudioSource => Owner.AudioSource;

        public PromptToneClip(float startTime, float endTime, PromptToneTrack owner) :
            base(startTime, endTime, owner)
        {
        }
    }
}
