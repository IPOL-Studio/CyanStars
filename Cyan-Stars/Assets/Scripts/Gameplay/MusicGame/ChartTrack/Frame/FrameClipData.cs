using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameClipData
    {
        public FrameChartTrackData FrameChartTrackData;

        public BpmGroups.BeatToTimeDelegate BeatToTimeDelegate;

        public FrameClipData(FrameChartTrackData frameChartTrackData, BpmGroups.BeatToTimeDelegate beatToTimeDelegate)
        {
            this.FrameChartTrackData = frameChartTrackData;
            this.BeatToTimeDelegate = beatToTimeDelegate;
        }
    }
}
