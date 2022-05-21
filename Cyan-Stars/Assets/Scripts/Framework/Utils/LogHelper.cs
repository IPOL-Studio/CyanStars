using UnityEngine;
using CyanStars.Framework.Logger;

namespace CyanStars.Framework.Utils
{
    public static class LogHelper
    {
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
