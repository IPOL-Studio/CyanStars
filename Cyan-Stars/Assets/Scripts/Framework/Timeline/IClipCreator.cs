namespace CyanStars.Framework.Timeline
{
    public interface IClipCreator<TTrack>
        where TTrack : BaseTrack, new()
    {
        IClip<TTrack> Create(TTrack track, int curIndex);
    }
}
