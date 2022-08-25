using System;
using CyanStars.Framework.Pool;

namespace CyanStars.Gameplay.Dialogue
{
    public class PlayMusicEventArgs : EventArgs, IReference
    {
        public const string EventName = nameof(PlayMusicEventArgs);

        public string FilePath { get; private set; }

        public float FadeInTime { get; private set; }

        public float FadeOutTime { get; private set; }

        public bool IsCrossFading { get; private set; }


        public static PlayMusicEventArgs Create(string filePath, float fadeInTime, float fadeOutTime, bool isCrossFading)
        {
            PlayMusicEventArgs eventArgs = ReferencePool.Get<PlayMusicEventArgs>();
            eventArgs.FilePath = filePath;
            eventArgs.FadeInTime = fadeInTime;
            eventArgs.FadeOutTime = fadeOutTime;
            eventArgs.IsCrossFading = isCrossFading;

            return eventArgs;
        }

        public void Clear()
        {
            FilePath = default;
            FadeInTime = default;
            FadeOutTime = default;
            IsCrossFading = default;
        }
    }
}
