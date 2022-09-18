using System;

namespace CyanStars.Framework.Timer
{
    /// <summary>
    /// 定时器
    /// </summary>
    public struct Timer : IEquatable<Timer>
    {
        /// <summary>
        /// 目标时间
        /// </summary>
        public float TargetTime;

        /// <summary>
        /// 间隔
        /// </summary>
        public readonly float Interval;

        /// <summary>
        /// 剩余次数
        /// </summary>
        public int Counter;

        /// <summary>
        /// 定时回调
        /// </summary>
        public readonly TimerCallback Callback;

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public readonly object Userdata;

        public Timer(float targetTime,float interval, int counter, TimerCallback callback,object userdata)
        {
            TargetTime = targetTime;
            Interval = interval;
            Counter = counter;
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
            return Callback != null ? Callback.GetHashCode() : 0;
        }
    }
}
