using System;
using System.Runtime.CompilerServices;

namespace CatTimeline
{
    public interface ITrackBuilder<T, D> where T : BaseTrack
    {
        ITrackBuilder<T, D> AddClips(int clipCount, D userData, IClipCreator<T, D> clipCreator);
        ITrackBuilder<T, D> SortClip();
        ITrackBuilder<T, D> PostProcess(Action<T> action);
        TrackBuildResult<T> Build();
    }

    public struct TrackBuildResult<T> where T : BaseTrack
    {
        public T Track { get; private set; }

        public TrackBuildResult(T track)
        {
            Track = track;
        }

        public bool AddToTimeline(Timeline timeline)
        {
            return timeline.AddTrack(Track);
        }
    }

    internal class TrackBuilder<T, D> : ITrackBuilder<T, D> where T : BaseTrack, new()
    {
        [Flags]
        private enum BuildOperators
        {
            None = 0,
            AddClips = 1,
            SortClip = 1 << 1,
            PostProcess = 1 << 2
        }

        private BuildOperators buildOP = BuildOperators.None;
        private (int, D, IClipCreator<T, D>) createClipArgs;
        private Action<T> postProcessAction;

        private void AddClips(T track)
        {
            var (clipCount, userData, clipCreator) = createClipArgs;
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip<T> clip = clipCreator.CreateClip(track, i, userData);
                track.AddClip(clip);
            }
        }

        public ITrackBuilder<T, D> AddClips(int clipCount, D userData, IClipCreator<T, D> clipCreator)
        {
            createClipArgs = (clipCount, userData, clipCreator);
            buildOP |= BuildOperators.AddClips;
            return this;
        }

        public ITrackBuilder<T, D> SortClip()
        {
            buildOP |= BuildOperators.SortClip;
            return this;
        }

        public ITrackBuilder<T, D> PostProcess(Action<T> action)
        {
            postProcessAction = action;
            buildOP |= BuildOperators.PostProcess;
            return this;
        }

        public TrackBuildResult<T> Build()
        {
            T track = new T();
            if (ShouldExecute(BuildOperators.AddClips))
                AddClips(track);

            if (ShouldExecute(BuildOperators.SortClip))
                track.SortClip();

            if (ShouldExecute(BuildOperators.PostProcess))
                postProcessAction?.Invoke(track);

            return new TrackBuildResult<T>(track);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldExecute(BuildOperators ops) => (buildOP & ops) != 0;
    }

    public static class TrackHelper
    {
        public static ITrackBuilder<T, D> CreateBuilder<T, D>() where T : BaseTrack, new() =>
            new TrackBuilder<T, D>();

        public static ITrackBuilder<T, object> CreateBuilder<T>() where T : BaseTrack, new() =>
            new TrackBuilder<T, object>();
    }
}