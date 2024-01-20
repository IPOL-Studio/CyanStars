namespace CyanStars.Framework.Logging
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        /// <summary>
        /// for <see cref="UnityEngine.LogType.Assert"/>, please don't use
        /// </summary>
        Assert,
        Error,
        Exception,
        Unknown,
        None
    }

    public static class LogLevelExtensions
    {
        public static string ToUpperName(this LogLevel logLevel, bool isIgnoreUnknown = false) => logLevel switch
        {
                LogLevel.Debug     => "DEBUG",
                LogLevel.Info      => "INFO",
                LogLevel.Warning   => "WARNING",
                LogLevel.Assert    => "ASSERT",
                LogLevel.Error     => "ERROR",
                LogLevel.Exception => "EXCEPTION",
                _                  => isIgnoreUnknown ? string.Empty : "UNKNOWN",
        };
    }
}
