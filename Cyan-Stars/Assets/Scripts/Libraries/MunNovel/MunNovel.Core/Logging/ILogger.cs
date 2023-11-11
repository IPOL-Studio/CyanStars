using System;

namespace MunNovel.Logging
{
    public interface ILogger
    {
        void Log<TState>(LogLevel level, TState state, Exception exception, Func<TState, Exception, string> formatter);
        bool IsEnabled(LogLevel level);
    }
}