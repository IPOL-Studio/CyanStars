using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Logger
{
    public class LoggerManager : BaseManager
    {
        private readonly Dictionary<Type, ILogger> LoggerDict = new Dictionary<Type, ILogger>();

        public override int Priority { get; }

        /// <inheritdoc />
        public override void OnInit()
        {
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
        }

        public T GetOrCreateLogger<T>() where T : ILogger, new()
        {
            var type = typeof(T);
            if (LoggerDict.TryGetValue(type, out var logger))
            {
                return (T)logger;
            }

            logger = new T();
            LoggerDict.Add(type, logger);
            return (T)logger;
        }

        public T GetLogger<T>() where T : ILogger => (T)LoggerDict[typeof(T)];

        public bool TryGetLogger<T>(out T logger) where T : ILogger
        {
            var isFound = LoggerDict.TryGetValue(typeof(T), out var result);
            logger = isFound ? (T)result : default;
            return isFound;
        }

        public void AddLogger<T>(T logger) where T : ILogger
        {
            if (logger is null)
            {
                throw new NullReferenceException(nameof(logger));
            }

            LoggerDict.Add(typeof(T), logger);
        }

        public bool RemoveLogger<T>() where T : ILogger => LoggerDict.Remove(typeof(T));
    }
}
