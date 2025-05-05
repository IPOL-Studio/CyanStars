using System;

namespace CyanStars.Gameplay.MusicGame
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TrackLoaderAttribute : Attribute
    {
        public readonly Type ChartTrackDataType;
        public readonly Type GameplayTrackType;

        public TrackLoaderAttribute(Type chartTrackDataType, Type gameplayTrackType)
        {
            ChartTrackDataType = chartTrackDataType;
            GameplayTrackType = gameplayTrackType;
        }
    }
}
