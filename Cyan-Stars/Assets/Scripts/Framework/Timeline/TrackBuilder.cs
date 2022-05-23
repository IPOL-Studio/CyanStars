using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CyanStars.Framework.Timeline
{
    public readonly struct TrackBuildResult<T> where T : BaseTrack
    {
        public readonly T Track;

        public TrackBuildResult(T track)
        {
            Track = track;
        }

        public bool AddToTimeline(Timeline timeline)
        {
            return timeline.AddTrack(Track);
        }
    }

    public class TrackBuilder<T, D> : ITrackBuilder<T> where T : BaseTrack, new()
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
        private ITrackBuilderClipCreator<T> clipCreatorHandle;
        private Action<T> postProcessAction;
        
        internal TrackBuilder() { }

        internal void AddClips(ITrackBuilderClipCreator<T> creator)
        {
            clipCreatorHandle = creator;
            buildOP = BuildOperators.AddClips;
        }

        public ITrackBuilder<T> AddClips(int clipCount, D userData, IClipCreator<T, D> clipCreator)
        {
            AddClips(new TrackBuilderClipCreator<T, D>(clipCount, userData, clipCreator));
            return this;
        }

        ITrackBuilder<T> ITrackBuilder<T>.SortClip()
        {
            buildOP |= BuildOperators.SortClip;
            return this;
        }

        ITrackBuilder<T> ITrackBuilder<T>.PostProcess(Action<T> action)
        {
            postProcessAction = action;
            buildOP |= BuildOperators.PostProcess;
            return this;
        }

        TrackBuildResult<T> ITrackBuilder<T>.Build()
        {
            T track = new T();
            if (ShouldExecute(BuildOperators.AddClips))
                clipCreatorHandle.Execute(track);

            if (ShouldExecute(BuildOperators.SortClip))
                track.SortClip();

            if (ShouldExecute(BuildOperators.PostProcess))
                postProcessAction?.Invoke(track);

            return new TrackBuildResult<T>(track);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldExecute(BuildOperators ops) => (buildOP & ops) != 0;
    }

    public class TrackBuilder_A<T, TItem> where T : BaseTrack, new()
    {
        internal TrackBuilder_A() { }

        public ITrackBuilder<T> AddClips(ICollection<TItem> userData, IClipCreator<T, ICollection<TItem>> clipCreator) =>
            new TrackBuilder<T, ICollection<TItem>>().AddClips(userData.Count, userData, clipCreator);

        public ITrackBuilder<T> AddClips(IList<TItem> userData, IClipCreator<T, IList<TItem>> clipCreator) =>
            new TrackBuilder<T, IList<TItem>>().AddClips(userData.Count, userData, clipCreator);

        public ITrackBuilder<T> AddClips(TItem[] userData, IClipCreator<T, TItem[]> clipCreator) =>
            new TrackBuilder<T, TItem[]>().AddClips(userData.Length, userData, clipCreator);
    }

    public class TrackBuilder_I<T, TItem> where T : BaseTrack, new()
    {
        internal TrackBuilder_I() { }

        public ITrackBuilder<T> AddClips(IEnumerable<TItem> userData, IClipCreatorForEach<T, TItem> clipCreator)
        {
            var builder = new TrackBuilder<T, IEnumerable<TItem>>();
            builder.AddClips(new TrackBuilderClipCreatorForEach<T, TItem>(userData, clipCreator));
            return builder;
        }

        public ITrackBuilder<T> AddClips(IList<TItem> userData, IClipCreatorForEach<T, TItem> clipCreator)
        {
            var builder = new TrackBuilder<T, IList<TItem>>();
            builder.AddClips(new TrackBuilderClipCreatorForEach<T, TItem>(userData, clipCreator));
            return builder;
        }
    }

    public static class TrackBuilderExtension
    {
        public static ITrackBuilder<T> AddClips<T, D>(this TrackBuilder<T, D> builder, D userData, IClipCreator<T, D> clipCreator)
            where T : BaseTrack, new()
            where D : IClipCount
        {
            return builder.AddClips(userData.ClipCount, userData, clipCreator);
        }
    }
}