using System;
using System.Runtime.CompilerServices;

namespace CatTimeline
{
    /// <summary>
    /// 片段创建函数原型
    /// </summary>
    public delegate BaseClip<T> CreateClipFunc<T>(T track, int clipIndex, object userData) where T : BaseTrack;

    public interface ITrackBuilder<T> where T : BaseTrack
    {
        ITrackBuilder<T> AddClips(int clipCount, object userData, CreateClipFunc<T> func);
        ITrackBuilder<T> SortClip();
        ITrackBuilder<T> PostProcess(Action<T> action);
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

    internal class TrackBuilder<T> : ITrackBuilder<T> where T : BaseTrack, new()
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
        private (int, object, CreateClipFunc<T>) createClipArgs;
        private Action<T> postProcessAction;

        private void AddClips(T track)
        {
            var (clipCount, userData, func) = createClipArgs;
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip<T> clip = func(track, i, userData);
                track.AddClip(clip);
            }
        }

        public ITrackBuilder<T> AddClips(int clipCount, object userData, CreateClipFunc<T> func)
        {
            createClipArgs = (clipCount, userData, func);
            buildOP |= BuildOperators.AddClips;
            return this;
        }

        public ITrackBuilder<T> SortClip()
        {
            buildOP |= BuildOperators.SortClip;
            return this;
        }

        public ITrackBuilder<T> PostProcess(Action<T> action)
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
        public static ITrackBuilder<T> CreateBuilder<T>() where T : BaseTrack, new() =>
            new TrackBuilder<T>();
    }
}