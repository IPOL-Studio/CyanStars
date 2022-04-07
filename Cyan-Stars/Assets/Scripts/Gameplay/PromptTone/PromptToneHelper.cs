using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.PromptTone
{
    public class PromptToneHelper : SingletonMono<PromptToneHelper>
    {
        public AudioClip ns_ka;
        public AudioClip na_ding;

        public AudioClip GetAudioClipWithType(PromptToneType type) => type switch
        {
            PromptToneType.NsKa => ns_ka,
            PromptToneType.NaDing => na_ding,
            _ => null
        };
    }
}