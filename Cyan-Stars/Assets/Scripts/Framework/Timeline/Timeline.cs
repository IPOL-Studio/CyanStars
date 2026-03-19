#nullable enable

using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴
    /// </summary>
    public class Timeline
    {
        /// <summary>
        /// 轨道列表
        /// </summary>
        private readonly List<BaseTrack> Tracks;

        /// <summary>
        /// 时间轴播放状态上下文
        /// </summary>
        public readonly TimelineContext Context;

        /// <summary>
        /// 音游模式下时间轴停止回调
        /// </summary>
        /// <remarks>时间轴在 isMusicGameMode 模式下且 CurrentTime 大于等于 Length</remarks>
        public event Action? OnEndInMusicGameMode;


        public Timeline(float length)
        {
            Tracks = new List<BaseTrack>();
            Context = new TimelineContext(length, -float.Epsilon, -float.Epsilon);
        }


        #region Tracks

        /// <summary>
        /// 添加轨道
        /// </summary>
        public TTrack AddTrack<TTrack, TTrackData, TClipData>(TTrackData trackData,
                                                              CreateClipFunc<TTrack, TTrackData, TClipData> creator)
            where TTrack : BaseTrack, new()
            where TTrackData : ITrackData<TClipData>
        {
            TTrack track = TrackBuilder<TTrack, TTrackData, TClipData>.Build(trackData, creator);

            track.Owner = this;
            Tracks.Add(track);

            return track;
        }

        /// <summary>
        /// 获取轨道
        /// </summary>
        public BaseTrack? GetTrack(int index)
        {
            if (index < 0 || index >= Tracks.Count)
            {
                return null;
            }

            return Tracks[index];
        }

        /// <summary>
        /// 获取轨道
        /// </summary>
        public T? GetTrack<T>(int index) where T : BaseTrack
        {
            return GetTrack(index) as T;
        }

        /// <summary>
        /// 获取轨道（指定类型的第一个）
        /// </summary>
        public T? GetTrack<T>() where T : BaseTrack
        {
            for (int i = 0; i < Tracks.Count; i++)
            {
                BaseTrack track = Tracks[i];
                if (track.GetType() == typeof(T))
                {
                    return (T)track;
                }
            }

            return null;
        }

        #endregion

        #region Timeline

        /// <summary>
        /// 在播放时每帧传入 dspTime，timeline 内部将乘以播放速度，需要暂停 timeline 时停止传入 dspTime
        /// </summary>
        public void OnPlayingUpdate(double smoothDeltaDspTime)
        {
            if (smoothDeltaDspTime < 0)
                throw new ArgumentOutOfRangeException(nameof(smoothDeltaDspTime));

            if (smoothDeltaDspTime == 0)
                return;

            Context.PreviousTime = Context.CurrentTime;
            Context.CurrentTime += smoothDeltaDspTime;
            Context.CurrentTime = Math.Min(Context.CurrentTime, Context.Length);

            foreach (var track in Tracks)
                track.OnPlayingUpdate(Context);

            if (Context.CurrentTime >= Context.Length)
                OnEndInMusicGameMode?.Invoke();
        }

        /// <summary>
        /// 快进跳转到某个时间点，不支持跳回到旧时间
        /// </summary>
        public void SkipTo(double targetTime)
        {
            Context.PreviousTime = Context.CurrentTime;

            if (targetTime < Context.PreviousTime)
                throw new ArgumentOutOfRangeException(nameof(targetTime), "不允许 Skip 到之前的时间，请重建一个 timeline 实例后从零开始 Skip");

            Context.CurrentTime = targetTime;
            foreach (var track in Tracks)
                track.OnTimeSkip(Context);
        }

        #endregion
    }
}
