using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    internal interface ITrackBuilderClipCreator<T> where T : BaseTrack
    {
        void Execute(T track);
    }

    internal class TrackBuilderClipCreator<T, D> : ITrackBuilderClipCreator<T> where T : BaseTrack
    {
        private int clipCount;
        private D userData;
        private IClipCreator<T, D> clipCreator;

        public TrackBuilderClipCreator(int clipCount, D userData, IClipCreator<T, D> clipCreator)
        {
            this.clipCount = clipCount;
            this.userData = userData;
            this.clipCreator = clipCreator;
        }

        public void Execute(T track)
        {
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip<T> clip = clipCreator.CreateClip(track, i, userData);
                track.AddClip(clip);
            }
        }
    }

    internal class TrackBuilderClipCreatorForEach<T, TItem> : ITrackBuilderClipCreator<T> where T : BaseTrack
    {
        private class ListFor : ITrackBuilderClipCreator<T>
        {
            public IList<TItem> List;
            public IClipCreatorForEach<T, TItem> ClipCreator;

            public void Execute(T track)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    BaseClip<T> clip = ClipCreator.CreateClip(track, List[i]);
                    track.AddClip(clip);
                }
            }
        }

        private class EnumerableFor : ITrackBuilderClipCreator<T>
        {
            public IEnumerable<TItem> Enumerable;
            public IClipCreatorForEach<T, TItem> ClipCreator;

            public void Execute(T track)
            {
                foreach (var item in Enumerable)
                {
                    BaseClip<T> clip = ClipCreator.CreateClip(track, item);
                    track.AddClip(clip);
                }
            }
        }

        private ITrackBuilderClipCreator<T> creator;

        public TrackBuilderClipCreatorForEach(IList<TItem> list, IClipCreatorForEach<T, TItem> clipCreator)
        {
            creator = new ListFor { List = list, ClipCreator = clipCreator };
        }

        public TrackBuilderClipCreatorForEach(IEnumerable<TItem> enumerable, IClipCreatorForEach<T, TItem> clipCreator)
        {
            creator = new EnumerableFor { Enumerable = enumerable, ClipCreator = clipCreator };
        }

        public void Execute(T track)
        {
            creator.Execute(track);
        }
    }
}