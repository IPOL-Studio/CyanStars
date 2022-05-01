using UnityEngine;
using CyanStars.Framework.Loggers;

namespace CyanStars.Framework.Helpers
{
    public static class LogHelper
    {
        public static NoteLogger NoteLogger => NoteLogger.Instance;

        public static void Log(string message, LogLevelType logLevel)
        {
#if UNITY_EDITOR || CYS_LOGGER_ENABLE
            switch (logLevel)
            {
                case LogLevelType.Log:
                    Debug.Log(message);
                    break;
                case LogLevelType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevelType.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.LogError($"undefined Log, Type: {logLevel}, message: {message}");
                    break;
            }
#endif
        }
    }
}
