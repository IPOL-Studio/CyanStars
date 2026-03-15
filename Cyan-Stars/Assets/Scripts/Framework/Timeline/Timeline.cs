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


        public Timeline(bool isMusicGameMode, float length)
        {
            Tracks = new List<BaseTrack>();
            Context = new TimelineContext(isMusicGameMode, length, 1f, -float.Epsilon, -float.Epsilon);
        }


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

        #region Tracks

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

            if (smoothDeltaDspTime == 0 || Context.PlaybackSpeed == 0)
                return;

            Context.PreviousTime = Context.CurrentTime;
            Context.CurrentTime += smoothDeltaDspTime * Context.PlaybackSpeed; // 对 smoothDeltaDspTime 乘以播放倍速
            Context.CurrentTime = smoothDeltaDspTime * Context.PlaybackSpeed > 0
                ? Math.Min(Context.CurrentTime, Context.Length) // 如果正向播放，确保当前时间不大于 Length
                : Math.Max(Context.CurrentTime, 0); // 如果反向播放，确保当前时间不小于 0

            foreach (var track in Tracks)
                track.OnPlayingUpdate(Context);

            if (Context.IsMusicGameMode && Context.CurrentTime >= Context.Length)
                OnEndInMusicGameMode?.Invoke();
        }

        /// <summary>
        /// 在制谱器模式下跳转到某个时间点 (s)
        /// </summary>
        /// <remarks>
        /// 跳转时没有限制时间点在 [0,Length] 范围内，播放时根据播放倍速正负值可能会再次钳制
        /// </remarks>
        public void JumpTo(double targetTime)
        {
            Context.PreviousTime = Context.CurrentTime;
            Context.CurrentTime = targetTime;
            foreach (var track in Tracks)
                track.OnTimeJump(Context);
        }

        #endregion
    }
}
