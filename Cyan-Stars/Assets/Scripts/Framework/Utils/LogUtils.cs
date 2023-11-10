using CyanStars.Framework.Logging;

namespace CyanStars.Framework.Utils
{
    public class LogUtils
    {
        public static LogLevel ConvertToLogLevel(UnityEngine.LogType logType)
        {
            return logType switch
            {
                UnityEngine.LogType.Log       => LogLevel.Info,
                UnityEngine.LogType.Warning   => LogLevel.Warning,
                UnityEngine.LogType.Assert    => LogLevel.Assert,
                UnityEngine.LogType.Error     => LogLevel.Error,
                UnityEngine.LogType.Exception => LogLevel.Exception,
                _                             => LogLevel.Unknown
            };
        }

        public static UnityEngine.LogType ConvertToUnityLogType(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Warning   => UnityEngine.LogType.Warning,
                LogLevel.Assert    => UnityEngine.LogType.Assert,
                LogLevel.Error     => UnityEngine.LogType.Error,
                LogLevel.Exception => UnityEngine.LogType.Exception,
                _                  => UnityEngine.LogType.Log
            };
        }
    }
}
