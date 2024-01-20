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
            return unityLogger.IsLogTypeAllowed(LogLevelConverter.ToUnityLogType(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, in LogEntry<TState> logEntry)
        {
            string message;
            string stackTrace;

            if (logEntry.State is Exception e)
            {
                message = CreateExceptionString(e, IsTraceStack(logLevel));
                stackTrace = null;
            }
            else
            {
                message = logEntry.State.ToString();
                stackTrace = IsTraceStack(logLevel) ? CreateStackTraceString(new StackTrace(1, true)) : null;
            }

            LoggerHack.LogFormat(
                logger: unityLogger,
                logType: LogLevelConverter.ToUnityLogType(logLevel),
                logOptions: LogOption.NoStacktrace,
                context: logEntry.Context,
                format: stackTrace == null ? "[{0} / {1}] {2}" : "[{0} / {1}] {2}\n{3}",
                this.categoryName, logLevel.ToUpperName(), message, stackTrace);
        }

        private bool IsTraceStack(LogLevel logLevel) => logLevel >= this.minTraceLevel;

        private string CreateStackTraceString(StackTrace stackTrace)
        {
            if (stackTrace is null)
                return null;

            var sb = StringBuilderCache.Acquire();

            try
            {
                StackTraceHelper.AppendStackTraceString(stackTrace, sb, false);
            }
            catch (Exception e)
            {
                StringBuilderCache.Release(sb);
                this.unityLogger.LogException(new InvalidOperationException("Failed to create stack trace string.", e));
                return null;
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private string CreateExceptionString(Exception exception, bool isTraceStack)
        {
            var sb = StringBuilderCache.Acquire();

            try
            {
                var formatter = new UnityDebugExceptionFormatter(exception, sb, isTraceStack);
                formatter.FillString();
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
