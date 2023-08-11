namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 匿名创建片段的原型
    /// </summary>
    public delegate IClip<TTrack> CreateClipFunc<TTrack, in TTrackData, in TClipData>(TTrack track,
        TTrackData trackData, int curIndex, TClipData clipData)
        where TTrack : BaseTrack, new()
        where TTrackData : ITrackData<TClipData>;

    /// <summary>
    /// 匿名创建使用 Key 的片段的原型
    /// </summary>
    public delegate TClip CreateKeyClipFunc<in TTrack, in TTrackData, in TClipData, out TClip>(TTrack track,
        TTrackData trackData, int curIndex, TClipData clipData)
        where TTrack : BaseTrack, new()
        where TClip : IClip<TTrack>, IKeyableClip
        where TTrackData : ITrackData<TClipData>;

    /// <summary>
    /// 匿名创建 Key 的原型
    /// </summary>
    public delegate TKey CreateKeyFunc<in TClip, in TKeyData, out TKey>(TClip owner, TKeyData keyData)
        where TClip : IClip, IKeyableClip
        where TKey : IKey<TClip>;
}
