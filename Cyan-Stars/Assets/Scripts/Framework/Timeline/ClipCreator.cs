namespace CyanStars.Framework.Timeline
{
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

        public IClip<TTrack> Create(TTrack track, int curIndex)
        {
            return Create(track, curIndex, TrackData.ClipDataList[curIndex]);
        }

        protected abstract IClip<TTrack> Create(TTrack track, int curIndex, TClipData clipData);
    }

    public sealed class AnonymousClipCreator<TTrack, TTrackData, TClipData> : BaseClipCreator<TTrack, TTrackData, TClipData>
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
