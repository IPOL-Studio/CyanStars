using System;
using CyanStars.Framework.Logging;

namespace CyanStars.Framework.Dialogue
{
    public class MunNovelLoggerProxy : MunNovel.Logging.ILogger, IDisposable
    {
        private string loggerName;
        private ICysLogger logger;

        private Action<string> disposeLogger;

        private bool isDisposed;

        public MunNovelLoggerProxy(string loggerName, Func<string, ICysLogger> createLoggerFunc, Action<string> disposeLoggerAction)
        {
            _ = createLoggerFunc ?? throw new ArgumentNullException(nameof(createLoggerFunc));

            this.loggerName = loggerName;
            this.disposeLogger = disposeLoggerAction;
            this.logger = createLoggerFunc(loggerName);
        }

        public bool IsEnabled(MunNovel.Logging.LogLevel level)
        {
            return logger.IsEnabled(GetCysLogLevel(level));
        }

        public void Log<TState>(MunNovel.Logging.LogLevel level, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(level))
                return;

            var levelType = GetCysLogLevel(level);

            if (formatter != null)
            {
                var message = formatter(state, exception);
                logger.Log(levelType, message);
            }
            else if (exception != null)
            {
                logger.Log(levelType, exception);
            }
            else
            {
                logger.Log(levelType, state);
            }
        }

        private static LogLevel GetCysLogLevel(MunNovel.Logging.LogLevel level)
        {
            return level switch
            {
                MunNovel.Logging.LogLevel.Debug       => LogLevel.Debug,
                MunNovel.Logging.LogLevel.Information => LogLevel.Info,
                MunNovel.Logging.LogLevel.Warning     => LogLevel.Warning,
                MunNovel.Logging.LogLevel.Error       => LogLevel.Error,
                MunNovel.Logging.LogLevel.Critical    => LogLevel.Exception,
                _                                     => LogLevel.Unknown
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                logger = null;
                disposeLogger?.Invoke(loggerName);

                isDisposed = true;
            }
        }

        ~MunNovelLoggerProxy()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
