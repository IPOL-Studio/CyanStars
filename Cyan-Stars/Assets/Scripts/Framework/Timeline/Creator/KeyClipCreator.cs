namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// Key clip 创建者基类
    /// </summary>
    public abstract class BaseKeyClipCreator<TTrack, TClip, TTrackData, TClipData, TKeyData> :
        IClipCreator<TTrack>
        where TTrack : BaseTrack, new()
        where TClip : IClip<TTrack>, IKeyableClip
        where TTrackData : ITrackData<TClipData>
        where TClipData : IKeyClipData<TKeyData>
    {
        public TTrackData TrackData { get; set; }

        protected BaseKeyClipCreator()
        {

        }

        protected BaseKeyClipCreator(TTrackData trackData)
        {
            TrackData = trackData;
        }

        /// <inheritdoc />
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

        /// <inheritdoc cref="Create" />
        /// <param name="clipData">当前创建片段对应的片段数据</param>
        protected abstract TClip CreateClip(TTrack track, int curIndex, TClipData clipData);

        /// <summary>
        /// 创建 clip key
        /// </summary>
        /// <param name="owner">目标 clip</param>
        /// <param name="data">当前创建 key 对应的数据</param>
        /// <returns></returns>
        protected abstract IKey CreateKey(TClip owner, TKeyData data);
    }

    /// <summary>
    /// Key clip 匿名创建者
    /// </summary>
    internal sealed class AnonymousBaseKeyClipCreator<TTrack, TClip, TKey, TTrackData, TClipData, TKeyData> :
        BaseKeyClipCreator<TTrack, TClip, TTrackData, TClipData, TKeyData>
        where TTrack : BaseTrack, new()
        where TClip : IClip<TTrack>, IKeyableClip
        where TKey : IKey<TClip>
        where TTrackData : ITrackData<TClipData>
        where TClipData : IKeyClipData<TKeyData>
    {
        public CreateKeyClipFunc<TTrack, TTrackData, TClipData, TClip> ClipCreator { get; set; }
        public CreateKeyFunc<TClip, TKeyData, TKey> KeyCreator { get; set; }

        public AnonymousBaseKeyClipCreator(TTrackData trackData,
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
