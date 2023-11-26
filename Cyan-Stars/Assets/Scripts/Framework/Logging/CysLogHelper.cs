using UnityEngine;

namespace CyanStars.Framework.Logging
{
    [HideInStackTrace]
    public static class CysLogHelper
    {
        public static void LogDebug(string message, ICysLogger logger = null, Object context = null)
        {
            if (logger is null)
                Debug.Log(message, context);
            else
                logger.LogDebug(message, context);
        }

        public static void LogInfo(string message, ICysLogger logger = null, Object context = null)
        {
            if (logger is null)
                Debug.Log(message, context);
            else
                logger.LogInfo(message, context);
        }

        public static void LogWarning(string message, ICysLogger logger = null, Object context = null)
        {
            if (logger is null)
                Debug.LogWarning(message, context);
            else
                logger.LogWarning(message, context);
        }

        public static void LogError(string message, ICysLogger logger = null, Object context = null)
        {
            if (logger is null)
                Debug.LogError(message, context);
            else
                logger.LogError(message, context);
        }

        public static void LogException(System.Exception exception, ICysLogger logger = null, Object context = null)
        {
            if (logger is null)
                Debug.LogException(exception, context);
            else
                logger.LogException(exception, context);
        }
    }
}
