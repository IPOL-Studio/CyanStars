using System;
using System.Collections;
using System.Collections.Generic;

namespace CatTimeline
{
    /// <summary>
    /// 片段创建函数原型
    /// </summary>
    public delegate BaseClip<T> CreateClipFunc<T>(T track, int clipIndex,object userData) where T : BaseTrack;
    
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
        public int AddTrack<T>(int clipCount,object userData,CreateClipFunc<T> func) where T : BaseTrack, new()
        {
            T track = new T();
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip<T> clip = func(track,i,userData);
                track.AddClip(clip);
            }
            track.SortClip();
            track.Owner = this;
            
            tracks.Add(track);

            return tracks.Count - 1;
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
            BaseTrack track = GetTrack(index);
            return track as T;
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
            if (CurrentTime >= Length)
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

