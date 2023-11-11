using System;
using System.Diagnostics;
using UnityEngine;
using UnityLoggingHack;
using CyanStars.Framework.Utils;

namespace CyanStars.Framework.Logging
{
    internal sealed class UnityDebugLogger : ICysLogger
    {
        private ILogger unityLogger;
        private string categoryName;
        private LogLevel minTraceLevel;

        public UnityDebugLogger(ILogger unityLogger, string categoryName, LogLevel minTraceLevel)
        {
            this.unityLogger = unityLogger ?? throw new ArgumentNullException(nameof(unityLogger));
            this.categoryName = categoryName;
            this.minTraceLevel = minTraceLevel;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return unityLogger.IsLogTypeAllowed(LogUtils.ConvertToUnityLogType(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, in LogEntry<TState> logEntry)
        {
            string message;
            string stackTrace;

            if (logEntry.State is Exception e)
            {
                message = e.Message;
                stackTrace = IsTraceStack(LogLevel.Exception) ? CreateStackTraceString(new StackTrace(e, true)) : null;
                logLevel = LogLevel.Exception;
            }
            else
            {
                message = logEntry.State.ToString();
                stackTrace = CreateStackTraceString(IsTraceStack(logEntry.LogLevel) ? new StackTrace(1, true) : null);
            }

            LoggerHack.LogFormat(
                logger: unityLogger,
                logType: LogUtils.ConvertToUnityLogType(logLevel),
                logOptions: LogOption.NoStacktrace,
                context: logEntry.Context,
                format: stackTrace == null ? "[{0} / {1}] {2}" : "[{0} / {1}] {2}\n{3}",
                this.categoryName, logLevel.ToUpperString(), message, stackTrace);
        }

        private bool IsTraceStack(LogLevel logLevel) => logLevel >= this.minTraceLevel;

        private string CreateStackTraceString(StackTrace stackTrace)
        {
            if (stackTrace is null)
                return null;

            var sb = StringBuilderCache.Acquire();

            try
            {
                StackTraceHelper.AppendStackTraceString(stackTrace, sb);
            }
            catch (Exception e)
            {
                StringBuilderCache.Release(sb);
                this.unityLogger.LogException(new InvalidOperationException("Failed to create stack trace string.", e));
                return null;
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}
