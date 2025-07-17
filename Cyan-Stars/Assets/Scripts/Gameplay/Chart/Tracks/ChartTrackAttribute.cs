using System;

namespace CyanStars.Gameplay.Chart
{
    public class ChartTrackAttribute : Attribute
    {
        public readonly string TrackKey;

        public ChartTrackAttribute(string trackKey)
        {
            TrackKey = trackKey;
        }
    }
}
