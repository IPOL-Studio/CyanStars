using System;
using UnityEngine;

namespace CyanStars.Framework.Logging
{
    internal sealed class UnityDebugLoggerProvider : ILoggerProvider
    {
        private readonly UnityEngine.ILogger UnityLogger;

        public UnityDebugLoggerProvider(UnityEngine.ILogger unityLogger)
        {
            UnityLogger = unityLogger ?? throw new ArgumentNullException(nameof(unityLogger));
        }

        public void Dispose()
        {
        }

        public ICysLogger CreateLogger(string categoryName)
        {
            _ = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            return new UnityDebugLogger(UnityLogger, categoryName, Application.isEditor ? LogLevel.Debug : LogLevel.Info);
        }
    }
}
