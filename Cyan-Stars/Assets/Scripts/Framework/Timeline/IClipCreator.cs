namespace CyanStars.Framework.Timeline
{
    public interface IClipCreator<T, D> where T : BaseTrack
    {
        BaseClip<T> CreateClip(T track, int clipIndex, D userData);
    }

    public interface IClipCreatorForEach<T, TItem> where T : BaseTrack
    {
        BaseClip<T> CreateClip(T track, TItem item);
    }
}