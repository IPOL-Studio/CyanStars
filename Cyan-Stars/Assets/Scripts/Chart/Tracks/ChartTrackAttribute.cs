using System;

namespace CyanStars.Chart.Tracks
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
