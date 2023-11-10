using System;

namespace CyanStars.Framework.Logging
{
    public readonly struct LogEntry<T>
    {
        public readonly LogLevel LogLevel;
        public readonly DateTime Timestamp;
        public readonly T State;
        public readonly UnityEngine.Object Context;

        public LogEntry(LogLevel logLevel, DateTime timestamp, T state, UnityEngine.Object context = null)
        {
            LogLevel      = logLevel;
            Timestamp     = timestamp;
            State         = state;
            Context       = context;
        }
    }
}
