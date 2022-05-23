using UnityEngine;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.PromptTone
{
    public class PromptToneHelper : SingletonMono<PromptToneHelper>
    {
        [SerializeField]
        private AudioClip nsKa;

        [SerializeField]
        private AudioClip naDing;

        public AudioClip GetAudioClipWithType(PromptToneType type) => type switch
        {
            PromptToneType.NsKa => nsKa,
            PromptToneType.NaDing => naDing,
            _ => null
        };
    }
}
