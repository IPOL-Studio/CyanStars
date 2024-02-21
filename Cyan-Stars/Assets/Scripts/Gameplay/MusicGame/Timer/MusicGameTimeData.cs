using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public readonly struct MusicGameTimeData
    {
        public readonly double RealtimeSinceStartup;
        public readonly double UnscaledDeltaTime;

        private MusicGameTimeData(double realtimeSinceStartup, double unscaledDeltaTime)
        {
            RealtimeSinceStartup = realtimeSinceStartup;
            UnscaledDeltaTime = unscaledDeltaTime;
        }

        public static MusicGameTimeData Create()
        {
            return new MusicGameTimeData(Time.realtimeSinceStartupAsDouble, Time.unscaledDeltaTime);
        }
    }
}
