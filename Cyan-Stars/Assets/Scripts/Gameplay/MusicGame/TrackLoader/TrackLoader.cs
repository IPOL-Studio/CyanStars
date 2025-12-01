using CyanStars.Framework.Timeline;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public interface ITrackLoader
    {
        public bool IsEnabled { get; }
        public void LoadTrack(Timeline timeline, ChartData chartData, int trackIndex);
    }

    public abstract class BaseTrackLoader<TChartTrackData> : ITrackLoader
        where TChartTrackData : IChartTrackData
    {
        public virtual bool IsEnabled => true;

        public void LoadTrack(Timeline timeline, ChartData chartData, int trackIndex)
        {
            var accessor = new ChartTrackAccessor<TChartTrackData>(trackIndex);
            LoadTrack(timeline, chartData, accessor);
        }

        public abstract void LoadTrack(Timeline timeline, ChartData chartData, ChartTrackAccessor<TChartTrackData> trackAccessor);
    }
}
