using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Timer
{
    public delegate void UpdateTimerCallback(float deltaTime, object userdata);

    public sealed class UpdateTimer : ITimer
    {
        private readonly struct Timer : IEquatable<Timer>
        {
            /// <summary>
            /// 定时回调
            /// </summary>
            public readonly UpdateTimerCallback Callback;

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public readonly object Userdata;

            public Timer(UpdateTimerCallback callback, object userdata)
            {
                Callback = callback;
                Userdata = userdata;
            }

            public bool Equals(Timer other)
            {
                return Callback == other.Callback;
            }

            public override bool Equals(object obj)
            {
                return obj is Timer other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Callback?.GetHashCode() ?? 0;
            }
        }


        private List<Timer> timers = new List<Timer>();

        public void OnUpdate(float deltaTime)
        {
            if (timers.Count > 0)
            {
                for (int i = timers.Count - 1; i >= 0; i--)
                {
                    Timer timer = timers[i];
                    timer.Callback?.Invoke(deltaTime, timer.Userdata);
                }
            }
        }

        /// <summary>
        /// 添加Update定时器
        /// </summary>
        public void Add(UpdateTimerCallback callback, object userdata = null)
        {
            Timer timer = new Timer(callback, userdata);
            if (timers.Contains(timer))
            {
                Debug.LogError("重复添加了Update定时器");
                return;
            }

            timers.Add(timer);
        }

        /// <summary>
        /// 移除Update定时器
        /// </summary>
        public void Remove(UpdateTimerCallback callback)
        {
            Timer timer = new Timer(callback, null);
            timers.Remove(timer);
        }
    }
}
