using UnityEngine;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音乐片段
    /// </summary>
    public class MusicClip : BaseClip<MusicTrack>
    {
        private AudioClip music;

        public MusicClip(float startTime, float endTime, MusicTrack owner, AudioClip music) :
            base(startTime, endTime, owner)
        {
            this.music = music;
        }

        public override void OnEnter()
        {
            Owner.AudioSource.clip = music;
            Owner.AudioSource.Play();
        }
    }
}
