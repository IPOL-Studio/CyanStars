using System;

namespace CyanStars.Framework.Timeline
{
    public interface ITrackBuilder<T> where T : BaseTrack
    {
        ITrackBuilder<T> SortClip();
        ITrackBuilder<T> PostProcess(Action<T> action);
        TrackBuildResult<T> Build();
    }
}