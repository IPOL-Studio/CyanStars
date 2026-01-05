using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    [TrackLoader(typeof(FrameChartTrackData))]
    public sealed class FrameTrackLoader : BaseTrackLoader<FrameChartTrackData>
    {
        public override bool IsEnabled => GameRoot.GetDataModule<MusicGameSettingsModule>().EnableFrameTrack;

        public override void LoadTrack(Timeline timeline, List<BpmItem> bpmGroup, ChartData chartData,
            ChartTrackAccessor<FrameChartTrackData> trackAccessor)
        {
            var frameClipData = new FrameClipData(
                trackAccessor.GetTrackData(chartData),
                bpmGroup
            );

            var frameTrackData = new FrameTrackData { ClipDataList = new List<FrameClipData> { frameClipData } };

            var track = timeline.AddTrack(frameTrackData, FrameTrack.CreateClipFunc);
            track.ImgFrame = GameRoot.UI.GetUIPanel<MusicGameMainPanel>().ImgFrame;
        }
    }
}
