using System;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 轨道构建器
    /// </summary>
    public static class TrackBuilder<TTrack,TTrackData,TClipData>  
        where TTrack : BaseTrack,new()
        where TTrackData : ITrackData<TClipData>
    {
        /// <summary>
        /// 构建轨道
        /// </summary>
        public static TTrack Build(TTrackData trackData,CreateClipFunc<TTrack, TTrackData, TClipData> creator)
        {
            TTrack track = new TTrack();

            for (int i = 0; i < trackData.ClipCount; i++)
            {
                TClipData clipData = trackData.ClipDataList[i];
                BaseClip<TTrack> clip = creator(track,trackData,i,clipData);
                track.AddClip(clip);
            }
            
            track.SortClip();
            
            return track;
        }
    }
}