using System;
using System.Runtime.CompilerServices;

namespace CyanStars.Framework.Logging
{
    public static class CysLoggerExtensions
    {
        public static void LogDebug<TState>(this ICysLogger logger, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Debug, state, context);
        }

        public static void LogInfo<TState>(this ICysLogger logger, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Info, state, context);
        }

        public static void LogWarning<TState>(this ICysLogger logger, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Warning, state, context);
        }

        public static void LogError<TState>(this ICysLogger logger, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Error, state, context);
        }

        public static void LogException<TState>(this ICysLogger logger, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Exception, state, context);
        }

        public static void LogException(this ICysLogger logger, Exception exception, UnityEngine.Object context = null)
        {
            logger.Log_Internal(LogLevel.Exception, exception, context);
        }

        public static void Log<TState>(this ICysLogger logger, LogLevel logLevel, TState state, UnityEngine.Object context = null)
        {
            logger.Log_Internal(logLevel, state, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Log_Internal<TState>(this ICysLogger logger, LogLevel logLevel, TState state, UnityEngine.Object context = null)
        {
            logger.Log(logLevel, CreateLogEntry(logLevel, state, context));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static LogEntry<TState> CreateLogEntry<TState>(LogLevel logLevel, TState state, UnityEngine.Object context = null)
        {
            return new LogEntry<TState>(
                logLevel:      logLevel,
                timestamp:     DateTime.Now,
                state:         state,
                context:       context
            );
        }
    }
}
