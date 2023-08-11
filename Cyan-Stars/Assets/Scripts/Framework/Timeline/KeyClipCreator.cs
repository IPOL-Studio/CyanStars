namespace CyanStars.Framework.Timeline
{
    public abstract class KeyClipCreator<TTrack, TClip, TTrackData, TClipData, TKeyData> :
        IClipCreator<TTrack>
        where TTrack : BaseTrack, new()
        where TClip : IClip<TTrack>, IKeyableClip
        where TTrackData : ITrackData<TClipData>
        where TClipData : IKeyClipData<TKeyData>
    {
        public TTrackData TrackData { get; set; }

        protected KeyClipCreator()
        {

        }

        protected KeyClipCreator(TTrackData trackData)
        {
            TrackData = trackData;
        }

        public IClip<TTrack> Create(TTrack track, int curIndex)
        {
            var clipData = TrackData.ClipDataList[curIndex];
            var clip = CreateClip(track, curIndex, clipData);

            for (int i = 0; i < clipData.KeyCount; i++)
            {
                IKey key = CreateKey(clip, clipData.KeyDataList[i]);
                clip.AddKey(key);
            }
            clip.SortKey();

            return clip;
        }

        protected abstract TClip CreateClip(TTrack track, int curIndex, TClipData clipData);

        protected abstract IKey CreateKey(TClip owner, TKeyData data);
    }

    public class AnonymousKeyClipCreator<TTrack, TClip, TKey, TTrackData, TClipData, TKeyData> :
        KeyClipCreator<TTrack, TClip, TTrackData, TClipData, TKeyData>
        where TTrack : BaseTrack, new()
        where TClip : IClip<TTrack>, IKeyableClip
        where TKey : IKey<TClip>
        where TTrackData : ITrackData<TClipData>
        where TClipData : IKeyClipData<TKeyData>
    {
        public CreateKeyClipFunc<TTrack, TTrackData, TClipData, TClip> ClipCreator { get; set; }
        public CreateKeyFunc<TClip, TKeyData, TKey> KeyCreator { get; set; }

        public AnonymousKeyClipCreator()
        {

        }

        public AnonymousKeyClipCreator(TTrackData trackData,
            CreateKeyClipFunc<TTrack, TTrackData, TClipData, TClip> clipCreator = null,
            CreateKeyFunc<TClip, TKeyData, TKey> keyCreator = null) : base(trackData)
        {
            this.ClipCreator = clipCreator;
            this.KeyCreator = keyCreator;
        }

        protected override TClip CreateClip(TTrack track, int curIndex, TClipData clipData)
        {
            return ClipCreator(track, TrackData, curIndex, clipData);
        }

        protected override IKey CreateKey(TClip owner, TKeyData data)
        {
            return KeyCreator(owner, data);
        }
    }
}
