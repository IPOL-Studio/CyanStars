using CyanStars.Framework.Helpers;

namespace CyanStars.Framework.Loggers
{
    public abstract class LoggerBase<T> : SingletonBase<T> where T : LoggerBase<T>, new()
    {
        public LogLevelType LogLevel { get; set; } = LogLevelType.Log;
    }
}
