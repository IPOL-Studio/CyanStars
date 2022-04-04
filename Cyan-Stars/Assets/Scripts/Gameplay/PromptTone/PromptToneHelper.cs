using UnityEngine;

namespace CyanStars.Gameplay.PromptTone
{
    public class PromptToneHelper : MonoBehaviour
    {
        public static PromptToneHelper Instance;

        public AudioClip ns_ka;
        public AudioClip na_ding;

        private void Start()
        {
            Instance = this;
        }

        public AudioClip GetAudioClipWithType(PromptToneType type) => type switch
        {
            PromptToneType.NsKa => ns_ka,
            PromptToneType.NaDing => na_ding,
            _ => null
        };
    }
}
