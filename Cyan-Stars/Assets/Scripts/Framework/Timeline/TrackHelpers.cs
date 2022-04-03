using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatTimeline
{
    /// <summary>
    /// 片段创建函数原型
    /// </summary>
    public delegate BaseClip<T> CreateClipFunc<T>(T track, int clipIndex,object userData) where T : BaseTrack;
    
    public interface ITrackBuilder<T> where T : BaseTrack
    {
        ITrackBuilder<T> AddClips(int clipCount, object userData, CreateClipFunc<T> func);
        ITrackBuilder<T> SortClip();
        ITrackBuilder<T> PostProcess(Action<T> action);
        bool AddToTimeline(Timeline timeline);
    }

    internal class TrackBuilder<T> : ITrackBuilder<T> where T : BaseTrack, new()
    {
        private T track = new T();

        public ITrackBuilder<T> AddClips(int clipCount, object userData, CreateClipFunc<T> func)
        {
            for (int i = 0; i < clipCount; i++)
            {
                BaseClip<T> clip = func(track, i, userData);
                track.AddClip(clip);
            }
            return this;
        }

        public ITrackBuilder<T> SortClip()
        {
            track.SortClip();
            return this;
        }
        
        public ITrackBuilder<T> PostProcess(Action<T> action)
        {
            action?.Invoke(track);
            return this;
        }

        public bool AddToTimeline(Timeline timeline)
        {
            return timeline.AddTrack(track);
        }
    }

    public static class TrackHelper
    {
        public static ITrackBuilder<T> CreateBuilder<T>() where T : BaseTrack, new() => 
            new TrackBuilder<T>();
    }
}