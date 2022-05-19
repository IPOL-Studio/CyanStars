using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Logger
{
    public static class LoggerManager
    {
        private static Dictionary<Type, LoggerBase> loggerDict = new Dictionary<Type, LoggerBase>();

        public static T GetOrCreateLogger<T>() where T : LoggerBase, new()
        {
            var type = typeof(T);
            if (loggerDict.TryGetValue(type, out var logger))
            {
                return (T)logger;
            }
            logger = new T();
            loggerDict.Add(type, logger);
            return (T)logger;
        }

        public static T GetLogger<T>() where T : LoggerBase => (T)loggerDict[typeof(T)];

        public static bool TryGetLogger<T>(out T logger) where T : LoggerBase
        {
            var isFound = loggerDict.TryGetValue(typeof(T), out var result);
            logger = isFound ? (T)result : default;
            return isFound;
        }

        public static void AddLogger<T>(T logger) where T : LoggerBase
        {
            if (logger is null)
            {
                throw new NullReferenceException(nameof(logger));
            }
            loggerDict.Add(typeof(T), logger);
        }

        public static bool RemoveLogger<T>() where T : LoggerBase => loggerDict.Remove(typeof(T));

        public static void Clear() => loggerDict.Clear();
    }
}