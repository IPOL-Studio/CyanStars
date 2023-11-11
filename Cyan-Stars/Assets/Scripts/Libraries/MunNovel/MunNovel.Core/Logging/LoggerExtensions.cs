using System;

namespace MunNovel.Logging
{
    public static class LoggerExtensions
    {
        private static readonly Func<string, Exception, string> _messageFormatter = (message, exception) => message;


        public static void LogDebug(this ILogger logger, string message) => logger.Log(LogLevel.Debug, message);
        public static void LogDebug(this ILogger logger, Exception exception) => logger.Log(LogLevel.Debug, exception);

        public static void LogInformation(this ILogger logger, string message) => logger.Log(LogLevel.Information, message);
        public static void LogInformation(this ILogger logger, Exception exception) => logger.Log(LogLevel.Information, exception);

        public static void LogWarning(this ILogger logger, string message) => logger.Log(LogLevel.Warning, message);
        public static void LogWarning(this ILogger logger, Exception exception) => logger.Log(LogLevel.Warning, exception);

        public static void LogError(this ILogger logger, string message) => logger.Log(LogLevel.Error, message);
        public static void LogError(this ILogger logger, Exception exception) => logger.Log(LogLevel.Error, exception);

        public static void LogCritical(this ILogger logger, string message) => logger.Log(LogLevel.Critical, message);
        public static void LogCritical(this ILogger logger, Exception exception) => logger.Log(LogLevel.Critical, exception);

        public static void Log(this ILogger logger, LogLevel level, string message) => logger.Log(level, message, null, _messageFormatter);
        public static void Log(this ILogger logger, LogLevel level, Exception exception) => logger.Log(level, null, exception, _messageFormatter);
    }
}
