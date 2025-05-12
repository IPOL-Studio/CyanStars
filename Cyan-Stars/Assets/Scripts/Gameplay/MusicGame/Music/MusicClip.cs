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

        /// <summary>
        /// 进入片段时，音乐从何时开始播放（s）
        /// </summary>
        private float prePlayTime;

        public MusicClip(float startTime, float endTime, float prePlayTime, MusicTrack owner, AudioClip music) : base(
            startTime, endTime, owner)
        {
            this.music = music;
            this.prePlayTime = prePlayTime;
        }

        public override void OnEnter()
        {
            Owner.AudioSource.clip = music;
            Owner.AudioSource.time = prePlayTime;
            Owner.AudioSource.Play();
        }
    }
}
