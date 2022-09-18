using System;

namespace CyanStars.Framework.Timer
{
    /// <summary>
    /// Update定时器
    /// </summary>
    public readonly struct UpdateTimer : IEquatable<UpdateTimer>
    {
        /// <summary>
        /// 定时回调
        /// </summary>
        public readonly UpdateTimerCallback Callback;

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public readonly object Userdata;

        public UpdateTimer(UpdateTimerCallback callback, object userdata)
        {
            Callback = callback;
            Userdata = userdata;
        }

        public bool Equals(UpdateTimer other)
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
