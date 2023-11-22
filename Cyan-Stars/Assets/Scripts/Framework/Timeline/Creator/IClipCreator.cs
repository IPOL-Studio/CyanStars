namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 创建片段接口
    /// </summary>
    /// <typeparam name="TTrack">片段可以放入的 Track 类型</typeparam>
    public interface IClipCreator<TTrack> where TTrack : BaseTrack, new()
    {
        /// <summary>
        /// 创建片段
        /// </summary>
        /// <param name="track">目标 track</param>
        /// <param name="curIndex">当前请求创建的片段 index</param>
        IClip<TTrack> Create(TTrack track, int curIndex);
    }
}
