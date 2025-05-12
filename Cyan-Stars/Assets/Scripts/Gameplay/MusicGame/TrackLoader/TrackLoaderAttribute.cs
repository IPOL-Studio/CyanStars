using System;

namespace CyanStars.Gameplay.MusicGame
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TrackLoaderAttribute : Attribute
    {
        public readonly Type ChartTrackDataType;

        public TrackLoaderAttribute(Type chartTrackDataType)
        {
            ChartTrackDataType = chartTrackDataType;
        }
    }
}
