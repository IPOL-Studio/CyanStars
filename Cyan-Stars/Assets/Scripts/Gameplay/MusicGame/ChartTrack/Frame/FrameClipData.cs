using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameClipData
    {
        public FrameChartTrackData FrameChartTrackData;
        public List<BpmGroupItem> BpmGroup;

        public FrameClipData(FrameChartTrackData frameChartTrackData, List<BpmGroupItem> bpmGroup)
        {
            this.FrameChartTrackData = frameChartTrackData;
            this.BpmGroup = bpmGroup;
        }
    }
}
