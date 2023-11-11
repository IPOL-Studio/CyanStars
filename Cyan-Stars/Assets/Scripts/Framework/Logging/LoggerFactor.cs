using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CyanStars.Framework.Utils;

namespace CyanStars.Framework.Logging
{
    // private nested classes
    public partial class LoggerFactor
    {
        private readonly struct LoggerInfo
        {
            public readonly string CategoryName;
            public readonly ICysLogger Logger;
            public readonly Type ProviderType;

            public LoggerInfo(ILoggerProvider provider, string categoryName)
            {
                CategoryName = categoryName;
                ProviderType = provider.GetType();
                Logger = provider.CreateLogger(categoryName);
            }
        }

        [HideInStackTrace]
        private sealed class Logger : ICysLogger
        {
            public LoggerInfo[] LoggerInfos;
            public UnityEngine.ILogger FallbackUnityLogger;

            public Logger(LoggerInfo[] loggerInfos, UnityEngine.ILogger fallbackUnityLogger)
            {
                this.LoggerInfos = loggerInfos;
                this.FallbackUnityLogger = fallbackUnityLogger;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                if (LoggerInfos == null)
                    return false;

                for (int i = 0; i < LoggerInfos.Length; i++)
                {
                    if (LoggerInfos[i].Logger?.IsEnabled(logLevel) == true)
                        return true;
                }

                return false;
            }

            public void Log<TState>(LogLevel logLevel, in LogEntry<TState> logEntry)
            {
                if (!IsEnabled(logLevel))
                    return;

                List<Exception> exceptions = null;

                for (int i = 0; i < LoggerInfos.Length; i++)
                {
                    ref var info = ref LoggerInfos[i];

                    if (info.Logger?.IsEnabled(logLevel) != true)
                        continue;

                    try
                    {
                        info.Logger.Log(logLevel, in logEntry);
                    }
                    catch (Exception e)
                    {
                        if (FallbackUnityLogger == null)
                            continue;

                        exceptions ??= new List<Exception>();
                        exceptions.Add(e);
                    }
                }

                if (exceptions != null && FallbackUnityLogger != null)
                {
                    FallbackUnityLogger.LogException(new AggregateException(exceptions), logEntry.Context);
                }
            }
        }

    }

    public partial class LoggerFactor
    {
        private UnityEngine.ILogger fallbackUnityLogger;

        private List<ILoggerProvider> loggerProviders = new List<ILoggerProvider>();
        private Dictionary<string, Logger> loggers = new Dictionary<string, Logger>();

        internal LoggerFactor(UnityEngine.ILogger fallbackUnityLogger) :
            this(Array.Empty<ILoggerProvider>(), fallbackUnityLogger)
        {
        }

        internal LoggerFactor(IEnumerable<ILoggerProvider> providers, UnityEngine.ILogger fallbackUnityLogger)
        {
            _ = providers ?? throw new ArgumentNullException(nameof(providers));
            this.fallbackUnityLogger = fallbackUnityLogger;

            foreach (var provider in providers)
            {
                loggerProviders.Add(provider);
            }
        }

        public ICysLogger GetOrCreateLogger(string categoryName)
        {
            ValidLoggerName(categoryName);

            if (!loggers.TryGetValue(categoryName, out Logger logger))
            {
                logger = new Logger(CreateLoggers(categoryName), fallbackUnityLogger);
                loggers.Add(categoryName, logger);
            }

            return logger;
        }

        public bool TryGetLogger(string categoryName, out ICysLogger logger)
        {
            if (loggers.TryGetValue(categoryName, out var loggerImpl))
            {
                logger = loggerImpl;
                return true;
            }

            logger = null;
            return false;
        }

        public ICysLogger GetLogger(string categoryName) =>
            loggers.GetValueOrDefault(categoryName);

        //TODO: maybe need dispose...
        public bool RemoveLogger(string categoryName) =>
            loggers.Remove(categoryName);

        private LoggerInfo[] CreateLoggers(string categoryName)
        {
            var infos = new LoggerInfo[loggerProviders.Count];

            for (int i = 0; i < loggerProviders.Count; i++)
            {
                infos[i] = new LoggerInfo(loggerProviders[i], categoryName);
            }

            return infos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidLoggerName(string name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("logger name can not be null or empty", nameof(name));
            }
        }
    }
}
