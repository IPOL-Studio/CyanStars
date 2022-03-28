using System;
using System.Collections;
using System.Collections.Generic;

namespace CatTimeline
{
    /// <summary>
    /// 轨道片段创建函数
    /// </summary>
    public delegate BaseClip CreateClipFunc(int clipIndex,object userData);
    
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
        /// 轨道片段创建函数字典
        /// </summary>
        private static Dictionary<Type, CreateClipFunc> createClipFuncDict = new Dictionary<Type, CreateClipFunc>();

        /// <summary>
        /// 注册轨道片段创建函数
        /// </summary>
        public static void RegisterCreateClipFunc<T>(CreateClipFunc func) where T : BaseTrack
        {
            createClipFuncDict[typeof(T)] = func;
        }
        
        /// <summary>
        /// 添加轨道
        /// </summary>
        public int AddTrack<T>(int clipCount,object userData) where T : BaseTrack, new()
        {
            Type type = typeof(T);
            if (!createClipFuncDict.TryGetValue(type,out CreateClipFunc func))
            {
                throw new Exception($"添加轨道失败，缺少轨道节点创建函数:{type.FullName}");
            }

            BaseTrack track = new T();
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip clip = func(i,userData);
                clip.Owner = track;
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
        public void Update(float deltaTime)
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
                track.Update(CurrentTime,previousTime);
            }

            if (CurrentTime >= Length)
            {
                OnStop?.Invoke();
            }
        }

        /// <summary>
        /// 重置时间轴
        /// </summary>
        public void Reset()
        {
            CurrentTime = -float.Epsilon;
        }
    }
}

