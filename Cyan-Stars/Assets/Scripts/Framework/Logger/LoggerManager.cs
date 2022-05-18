using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Logger
{
    public static class LoggerManager
    {
        private static Dictionary<Type, LoggerBase> loggerDict = new Dictionary<Type, LoggerBase>();

        public static T GetOrAddLogger<T>() where T : LoggerBase, new()
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