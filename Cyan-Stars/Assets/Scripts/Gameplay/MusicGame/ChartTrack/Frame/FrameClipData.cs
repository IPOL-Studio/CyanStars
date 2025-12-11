using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class FrameClipData
    {
        public FrameChartTrackData FrameChartTrackData;

        public BpmGroup.BeatToTimeDelegate BeatToTimeDelegate;

        public FrameClipData(FrameChartTrackData frameChartTrackData, BpmGroup.BeatToTimeDelegate beatToTimeDelegate)
        {
            this.FrameChartTrackData = frameChartTrackData;
            this.BeatToTimeDelegate = beatToTimeDelegate;
        }
    }
}
