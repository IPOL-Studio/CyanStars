namespace CyanStars.Framework.Timeline
{
    public static class TrackHelper
    {
        /// <summary>
        /// 使用任意数据源的Builder
        /// </summary>
        /// <typeparam name="T">Track</typeparam>
        /// <typeparam name="D">数据源</typeparam>
        public static TrackBuilder<T, D> CreateBuilder<T, D>() where T : BaseTrack, new() =>
            new TrackBuilder<T, D>();

        ///<summary>
        /// 创建使用数组作为数据源的Builder
        /// </summary>
        /// <typeparam name="T">Track</typeparam>
        /// <typeparam name="TItem">数据源Item</typeparam>
        public static TrackBuilder_A<T,TItem> CreateBuilder_A<T, TItem>() where T : BaseTrack, new() =>
            new TrackBuilder_A<T, TItem>();

        /// <summary>
        /// 使用迭代器传入数据的Builder
        /// </summary>
        /// <remarks>
        /// <see cref="IClipCreatorForEach{T,TItem}"/>
        /// </remarks>
        /// <typeparam name="T">Track</typeparam>
        /// <typeparam name="TItem">数据源Item</typeparam>
        public static TrackBuilder_I<T, TItem> CreateBuilder_I<T, TItem>() where T : BaseTrack, new() =>
            new TrackBuilder_I<T, TItem>();
    }
}