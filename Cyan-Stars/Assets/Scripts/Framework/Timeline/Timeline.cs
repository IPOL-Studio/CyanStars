using System;
using System.Collections.Generic;
using UnityEngine;

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
        public TimelineContext Context;

        /// <summary>
        /// 时间轴停止回调
        /// </summary>
        public event Action OnStop;


        public Timeline(bool isMusicGameMode, float length)
        {
            Tracks = new List<BaseTrack>();
            Context = new TimelineContext(isMusicGameMode, length, false, 1f, -float.Epsilon, -float.Epsilon);
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

        /// <summary>
        /// 获取轨道
        /// </summary>
        public BaseTrack GetTrack(int index)
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
        public T GetTrack<T>(int index) where T : BaseTrack
        {
            return GetTrack(index) as T;
        }

        /// <summary>
        /// 获取轨道（指定类型的第一个）
        /// </summary>
        public T GetTrack<T>() where T : BaseTrack
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

        /// <summary>
        /// 更新时间轴
        /// </summary>
        public void OnUpdate(double deltaTime)
        {
            if (deltaTime <= 0 || Context.CurrentTime >= Context.Length)
            {
                return;
            }

            Context.PreviousTime = Context.CurrentTime;
            Context.CurrentTime += deltaTime;
            foreach (var track in Tracks)
            {
                track.OnUpdate(in Context);
            }

            if (Context.CurrentTime >= Context.Length)
            {
                OnStop?.Invoke();
            }
        }
    }
}
