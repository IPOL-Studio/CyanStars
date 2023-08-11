namespace CyanStars.Framework.Timeline
{
    public interface IKeyableClip
    {
        void AddKey(IKey key);
        void SortKey();
    }
}
