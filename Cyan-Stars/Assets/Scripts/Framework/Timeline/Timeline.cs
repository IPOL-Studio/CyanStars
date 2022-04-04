using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴
    /// </summary>
    public class Timeline
    {
        public Timeline(float length)
        {
            Length = length;
        }

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime
        {
            get;
            private set;
        } = -float.Epsilon;

        /// <summary>
        /// 播放速度
        /// </summary>
        public float PlaybackSpeed
        {
            get;
            set;
        } = 1;
        
        /// <summary>
        /// 长度
        /// </summary>
        public readonly float Length;

        /// <summary>
        /// 时间轴停止回调
        /// </summary>
        public event Action OnStop;
        
        /// <summary>
        /// 轨道列表
        /// </summary>
        private List<BaseTrack> tracks = new List<BaseTrack>();
        
        /// <summary>
        /// 添加轨道
        /// </summary>
        public bool AddTrack<T>(T track) where T : BaseTrack
        {
            if (track is null)
                return false;

            if (track.Owner != null)
                throw new ArgumentException("track 已经被添加到了一个存在的 timeline 中");

            track.Owner = this;
            tracks.Add(track);
            return true;
        }

        /// <summary>
        /// 创建一个空轨道并添加至timeline
        /// </summary>
        public T AddTrack<T>() where T : BaseTrack, new()
        {
            T track = new T { Owner = this };
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
                track.OnUpdate(CurrentTime,previousTime);
            }

            if (CurrentTime >= Length)
            {
                OnStop?.Invoke();
            }
        }
        
    }
}

