using System;
using CyanStars.Framework.Pool;

namespace CyanStars.Framework.Event
{
    public class SingleEventArgs<T> : EventArgs, IReference
    {
        public T Value { get; private set; }

        public static SingleEventArgs<T> Create(T value)
        {
            SingleEventArgs<T> eventArgs = ReferencePool.Get<SingleEventArgs<T>>();
            eventArgs.Value = value;
            return eventArgs;
        }

        public void Clear()
        {
            Value = default;
        }
    }
}
