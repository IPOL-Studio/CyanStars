namespace CyanStars.Framework.Timeline
{

    /// <summary>
    /// 片段创建方法的原型
    /// </summary>
    public delegate BaseClip<TTrack> CreateClipFunc<TTrack, in TTrackData, in TClipData>(TTrack track,TTrackData trackData,int curIndex, TClipData clipData)
        where TTrack : BaseTrack,new()
        where TTrackData : ITrackData<TClipData>;
}