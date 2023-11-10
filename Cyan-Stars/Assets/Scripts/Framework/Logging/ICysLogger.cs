namespace CyanStars.Framework.Logging
{
    public interface ICysLogger
    {
        public void Log<TState>(LogLevel logLevel, in LogEntry<TState> logEntry);
        public bool IsEnabled(LogLevel logLevel);
    }
}
