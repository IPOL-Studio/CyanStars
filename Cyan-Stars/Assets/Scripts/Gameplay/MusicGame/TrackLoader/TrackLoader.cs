using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public interface ITrackLoader
    {
        public bool IsEnabled { get; }
        public void LoadTrack(Timeline timeline, List<BpmItem> bpmGroup, ChartData chartData, int trackIndex);
    }

    public abstract class BaseTrackLoader<TChartTrackData> : ITrackLoader
        where TChartTrackData : IChartTrackData
    {
        public virtual bool IsEnabled => true;

        public void LoadTrack(Timeline timeline, List<BpmItem> bpmGroup, ChartData chartData, int trackIndex)
        {
            var accessor = new ChartTrackAccessor<TChartTrackData>(trackIndex);
            LoadTrack(timeline, bpmGroup, chartData, accessor);
        }

        public abstract void LoadTrack(Timeline timeline, List<BpmItem> bpmGroup, ChartData chartData,
            ChartTrackAccessor<TChartTrackData> trackAccessor);
    }
}
