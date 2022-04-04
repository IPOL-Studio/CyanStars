namespace CatTimeline
{
    public interface IClipCreator<T, D> where T : BaseTrack
    {
        BaseClip<T> CreateClip(T track, int clipIndex, D userData);
    }
}