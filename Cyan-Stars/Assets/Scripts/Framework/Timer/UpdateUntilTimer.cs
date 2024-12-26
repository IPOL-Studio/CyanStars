using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Timer
{
    public delegate bool UpdateTimerUntilCallback(float deltaTime, object userdata);

    /// <summary>
    /// 在每个 Update 调用的计时器
    /// <para>直到 callback 返回 <see langword="true"/> 时，从计时器中移除</para>
    /// </summary>
    public sealed class UpdateUntilTimer : ITimer
    {
        private readonly struct Timer : IEquatable<Timer>
        {
            /// <summary>
            /// 定时回调
            /// </summary>
            public readonly UpdateTimerUntilCallback Callback;

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public readonly object Userdata;

            public Timer(UpdateTimerUntilCallback callback, object userdata)
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


        private TimerListContainer<Timer> runningTimers = new TimerListContainer<Timer>();

        public void OnUpdate(float deltaTime)
        {
            if (runningTimers.Count == 0)
            {
                return;
            }

            using var _ = runningTimers.Handle();

            for (int i = runningTimers.Count; i >= 0; i--)
            {
                if (runningTimers.TryGetValue(i, out Timer timer) &&
                    (timer.Callback?.Invoke(deltaTime, timer.Userdata) ?? true))
                {
                    runningTimers.RemoveAt(i);
                }
            }
        }

        public void Add(UpdateTimerUntilCallback callback, object userdata = null)
        {
            Timer timer = new Timer(callback, userdata);
            if (runningTimers.Contains(timer))
            {
                Debug.LogError("重复添加了UpdateUntil定时器");
                return;
            }

            runningTimers.Add(timer);
        }

        public void Remove(UpdateTimerUntilCallback callback)
        {
            Timer timer = new Timer(callback, null);
            runningTimers.Remove(timer);
        }
    }
}
