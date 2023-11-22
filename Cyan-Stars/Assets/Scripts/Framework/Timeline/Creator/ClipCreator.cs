namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// Clip 创建者基类
    /// </summary>
    public abstract class BaseClipCreator<TTrack, TTrackData, TClipData> : IClipCreator<TTrack>
        where TTrack : BaseTrack, new()
        where TTrackData : ITrackData<TClipData>
    {
        public TTrackData TrackData { get; set; }

        protected BaseClipCreator()
        {

        }

        protected BaseClipCreator(TTrackData trackData)
        {
            TrackData = trackData;
        }

        /// <inheritdoc />
        public IClip<TTrack> Create(TTrack track, int curIndex)
        {
            return Create(track, curIndex, TrackData.ClipDataList[curIndex]);
        }

        /// <inheritdoc cref="Create" />
        /// <param name="clipData">当前创建片段对应的片段数据</param>
        protected abstract IClip<TTrack> Create(TTrack track, int curIndex, TClipData clipData);
    }

    /// <summary>
    /// Clip 匿名创建者
    /// </summary>
    internal sealed class AnonymousClipCreator<TTrack, TTrackData, TClipData> : BaseClipCreator<TTrack, TTrackData, TClipData>
        where TTrack : BaseTrack, new()
        where TTrackData : ITrackData<TClipData>
    {
        public CreateClipFunc<TTrack, TTrackData, TClipData> Creator { get; set; }

        public AnonymousClipCreator(CreateClipFunc<TTrack, TTrackData, TClipData> creator)
        {
            Creator = creator;
        }

        public AnonymousClipCreator(TTrackData trackData, CreateClipFunc<TTrack, TTrackData, TClipData> creator)
            : base(trackData)
        {
            Creator = creator;
        }

        protected override IClip<TTrack> Create(TTrack track, int curIndex, TClipData clipData)
        {
            return Creator(track, TrackData, curIndex, clipData);
        }
    }
}
