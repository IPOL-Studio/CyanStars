using UnityEngine;

namespace UnityLoggingHack
{
    public static class LoggerHack
    {
        public static void LogFormat(ILogger logger, LogType logType, LogOption logOptions, Object context, string format, params object[] args)
        {
            if (logger.IsLogTypeAllowed(logType) && logger.logHandler is DebugLogHandler debugLogHandler)
            {
                debugLogHandler.LogFormat(logType, logOptions, context, format, args);
            }
        }
    }
}
