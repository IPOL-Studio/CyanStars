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


        private List<Timer> runningTimers = new List<Timer>();
        private List<Timer> waitRemoveTimers = new List<Timer>();

        public void OnUpdate(float deltaTime)
        {
            //处理UpdateUntil定时器
            if (runningTimers.Count > 0)
            {
                for (int i = runningTimers.Count - 1; i >= 0; i--)
                {
                    Timer timer = runningTimers[i];
                    if (timer.Callback?.Invoke(deltaTime, timer.Userdata) ?? true)
                    {
                        waitRemoveTimers.Add(timer);
                    }
                }
            }

            //删除等待删除的timer
            if (waitRemoveTimers.Count > 0)
            {
                foreach (Timer waitRemoveTimer in waitRemoveTimers)
                {
                    runningTimers.Remove(waitRemoveTimer);
                }

                waitRemoveTimers.Clear();
            }
        }

        public void Add(UpdateTimerUntilCallback callback, object userdata = null)
        {
            Timer timer = new Timer(callback, userdata);
            if (runningTimers.Contains(timer))
            {
                Debug.LogError("重复添加了UpdateUntil定时器");
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
