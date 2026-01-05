using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameClipData
    {
        public FrameChartTrackData FrameChartTrackData;
        public List<BpmItem> BpmGroup;

        public FrameClipData(FrameChartTrackData frameChartTrackData, List<BpmItem> bpmGroup)
        {
            this.FrameChartTrackData = frameChartTrackData;
            this.BpmGroup = bpmGroup;
        }
    }
}
