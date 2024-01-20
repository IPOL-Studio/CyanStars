using UnityEngine;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.MusicGame
{
    public class PromptToneHelper : SingletonMono<PromptToneHelper>
    {
        [SerializeField]
        private AudioClip nsKa;

        [SerializeField]
        private AudioClip nsDing;

        [SerializeField]
        private AudioClip nsTambourine;

        public AudioClip GetAudioClipWithType(PromptToneType type) => type switch
        {
            PromptToneType.NsKa => nsKa,
            PromptToneType.NsDing => nsDing,
            PromptToneType.NsTambourine => nsTambourine,
            _ => null
        };
    }
}
