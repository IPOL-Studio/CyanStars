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
        private List<BaseTrack> tracks = new List<BaseTrack>();

        /// <summary>
        /// 长度
        /// </summary>
        public readonly float Length;

        /// <summary>
        /// 时间轴停止回调
        /// </summary>
        public event Action OnStop;

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime { get; private set; } = -float.Epsilon;

        /// <summary>
        /// 播放速度
        /// </summary>
        public float PlaybackSpeed { get; set; } = 1;

        public Timeline(float length)
        {
            Length = length;
        }


        /// <summary>
        /// 添加轨道
        /// </summary>
        public TTrack AddTrack<TTrack>(IClipCreator<TTrack> creator, int clipCount)
            where TTrack : BaseTrack, new()
        {
            TTrack track = TrackBuilder<TTrack>.Build(clipCount, creator);

            track.Owner = this;
            tracks.Add(track);

            return track;
        }

        /// <summary>
        /// 获取轨道
        /// </summary>
        public BaseTrack GetTrack(int index)
        {
            if (index < 0 || index >= tracks.Count)
            {
                return null;
            }

            return tracks[index];
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
            for (int i = 0; i < tracks.Count; i++)
            {
                BaseTrack track = tracks[i];
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
        public void OnUpdate(float deltaTime)
        {
            if (deltaTime <= 0 || CurrentTime >= Length)
            {
                return;
            }

            float previousTime = CurrentTime;
            CurrentTime += deltaTime * PlaybackSpeed;
            for (int i = 0; i < tracks.Count; i++)
            {
                //更新轨道
                BaseTrack track = tracks[i];
                track.OnUpdate(CurrentTime, previousTime);
            }

            if (CurrentTime >= Length)
            {
                OnStop?.Invoke();
            }
        }
    }

    public static class TimelineExtension
    {
        /// <summary>
        /// 添加轨道
        /// </summary>
        public static TTrack AddTrack<TTrack, TTrackData, TClipData>(this Timeline timeline,
            TTrackData trackData,
            CreateClipFunc<TTrack, TTrackData, TClipData> clipCreator)
            where TTrack : BaseTrack, new()
            where TTrackData : ITrackData<TClipData>
        {
            var creator = new AnonymousClipCreator<TTrack, TTrackData, TClipData>(trackData, clipCreator);
            return timeline.AddTrack(creator, trackData.ClipCount);
        }

        /// <summary>
        /// 添加轨道
        /// </summary>
        public static TTrack AddTrack<TTrack, TClip, TKey, TTrackData, TClipData, TKeyData>(this Timeline timeline,
            TTrackData trackData,
            CreateKeyClipFunc<TTrack, TTrackData, TClipData, TClip> clipCreator,
            CreateKeyFunc<TClip, TKeyData, TKey> keyCreator)
            where TTrack : BaseTrack, new()
            where TClip : IClip<TTrack>, IKeyableClip
            where TKey : IKey<TClip>
            where TTrackData : ITrackData<TClipData>
            where TClipData : IKeyClipData<TKeyData>
        {
            var creator = new AnonymousBaseKeyClipCreator<TTrack, TClip, TKey, TTrackData, TClipData, TKeyData>(trackData, clipCreator, keyCreator);
            return timeline.AddTrack(creator, trackData.ClipCount);
        }
    }
}
