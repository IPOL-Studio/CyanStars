using System;
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


        private TimerListContainer<Timer> timers = new TimerListContainer<Timer>();

        public void OnUpdate(float deltaTime)
        {
            if (timers.Count == 0)
            {
                return;
            }

            using var _ = timers.Handle();

            for (int i = timers.Count; i >= 0; i--)
            {
                if (timers.TryGetValue(i, out Timer timer) && timer.Callback != null)
                {
                    timer.Callback(deltaTime, timer.Userdata);
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
