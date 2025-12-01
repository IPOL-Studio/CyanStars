using System;

namespace CyanStars.Chart
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
