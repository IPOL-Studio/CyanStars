using System;

namespace CyanStars.Gameplay.MusicGame
{
    public readonly struct GameTimeSpan
    {
        public readonly double TotalSeconds;
        public readonly int TotalMilliseconds;

        internal GameTimeSpan(double seconds, int milliseconds)
        {
            TotalSeconds = seconds;
            TotalMilliseconds = milliseconds;
        }

        public static GameTimeSpan FromSeconds(double value)
        {
            return new GameTimeSpan(value, (int)(value * 1000));
        }

        public static GameTimeSpan FromMilliseconds(int value)
        {
            return new GameTimeSpan(value / 1000.0, value);
        }

        public static GameTimeSpan FromTimeSpan(TimeSpan value)
        {
            return new GameTimeSpan(value.TotalSeconds, (int)value.TotalMilliseconds);
        }

        public static explicit operator TimeSpan(GameTimeSpan self)
        {
            return TimeSpan.FromSeconds(self.TotalSeconds);
        }
    }

    public static class GameTimeSpanExtensions
    {
        public static TimeSpan ToTimeSpan(this GameTimeSpan self)
        {
            return TimeSpan.FromSeconds(self.TotalSeconds);
        }
    }
}
