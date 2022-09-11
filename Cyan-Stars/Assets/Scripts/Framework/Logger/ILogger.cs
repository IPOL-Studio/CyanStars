namespace CyanStars.Framework.Logger
{
    public delegate void LogCallback(string message, LogLevelType logLevel);

    public interface ILogger
    {
        event LogCallback OnLog;
    }
}
