using System;
using CyanStars.Framework.Pool;

namespace CyanStars.Gameplay.Dialogue
{
    public class SetMusicEventArgs : EventArgs, IReference
    {
        public const string EventName = nameof(SetMusicEventArgs);

        public string FilePath { get; private set; }

        public float FadeInTime { get; private set; }

        public float FadeOutTime { get; private set; }



        public static SetMusicEventArgs Create(string filePath, float fadeInTime, float fadeOutTime)
        {
            SetMusicEventArgs eventArgs = ReferencePool.Get<SetMusicEventArgs>();
            eventArgs.FilePath = filePath;
            eventArgs.FadeInTime = fadeInTime;
            eventArgs.FadeOutTime = fadeOutTime;

            return eventArgs;
        }

        public void Clear()
        {
            FilePath = default;
            FadeInTime = default;
            FadeOutTime = default;
        }
    }
}
