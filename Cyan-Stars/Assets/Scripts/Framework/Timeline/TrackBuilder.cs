namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 轨道构建器
    /// </summary>
    public static class TrackBuilder<TTrack>
        where TTrack : BaseTrack, new()
    {
        /// <summary>
        /// 构建轨道
        /// </summary>
        public static TTrack Build(int clipCount, IClipCreator<TTrack> creator)
        {
            TTrack track = new TTrack();

            for (int i = 0; i < clipCount; i++)
            {
                IClip<TTrack> clip = creator.Create(track, i);
                track.AddClip(clip);
            }

            track.SortClip();

            return track;
        }
    }
}
