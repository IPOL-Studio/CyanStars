using CyanStars.Framework.Timeline;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class PromptToneKey : IKey<PromptToneClip>
    {
        IKeyableClip IKey.Owner => Owner;

        public PromptToneClip Owner { get; }
        public float Time { get; }

        private AudioClip promptTone;

        public PromptToneKey(PromptToneClip owner, float time, AudioClip promptTone)
        {
            this.Owner = owner;
            this.Time = time;
            this.promptTone = promptTone;
        }

        public void OnExecute(float currentTime)
        {
            Owner.AudioSource.PlayOneShot(promptTone);
        }
    }
}
