namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 可添加 Key 的片段接口
    /// </summary>
    public interface IKeyableClip
    {
        void AddKey(IKey key);
        void SortKey();
    }
}
