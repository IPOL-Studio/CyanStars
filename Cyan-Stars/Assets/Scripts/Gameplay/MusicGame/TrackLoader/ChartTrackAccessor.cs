using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public readonly struct ChartTrackAccessor<T>
    {
        public readonly int TrackIndex;

        public ChartTrackAccessor(int trackIndex)
        {
            this.TrackIndex = trackIndex;
        }

        public T GetTrackData(ChartData chartData) =>
            (T)chartData.TrackDatas[TrackIndex].TrackData;
    }
}
