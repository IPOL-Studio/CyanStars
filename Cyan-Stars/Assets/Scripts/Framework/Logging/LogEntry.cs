using System;

namespace CyanStars.Framework.Logging
{
    public readonly struct LogEntry<T>
    {
        public readonly DateTime Timestamp;
        public readonly T State;
        public readonly UnityEngine.Object Context;

        public LogEntry(DateTime timestamp, T state, UnityEngine.Object context = null)
        {
            Timestamp = timestamp;
            State     = state;
            Context   = context;
        }
    }
}
