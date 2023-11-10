using System;
using System.Collections.Generic;
using CyanStars.EditorExtension;
using CyanStars.Framework.Utils;
using UnityEngine;

namespace CyanStars.Framework.Logging
{
    // Init 后
    // Debug.Log -> UnityLoggerHandler -> UnityDebugLogger
    // UnityConsoleLogger log to Unity Editor Console
    // 通过以上流程拦截所有的 Log

    //整体 API 与 Microsoft.Extensions.Logging 类似
    // 可以同时输出到不同的 log target

    // 仅在 Runtime 生效

    public sealed class LoggerManager : BaseManager
    {
        private sealed class UnityLoggerHandler : ILogHandler
        {
            private readonly ICysLogger Logger;

            public UnityLoggerHandler(ICysLogger logger)
            {
                Logger = logger;
            }

            public void LogException(Exception exception, UnityEngine.Object context)
            {
                Logger.LogException(exception, context);
            }

            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                Logger.Log(LogUtils.ConvertToLogLevel(logType), string.Format(format, args), context);
            }
        }

        [SerializeField]
        [Header("是否使用 Cys Logger 替换默认的 Debug.Log (仅在编辑器内生效)")]
        [Active(ActiveMode.Edit)]
        private bool useCysConsoleLogger = true;

        public LoggerFactor LoggerFactor { get; private set; }

        public override int Priority { get; }

        /// <summary>
        /// 基于 Unity Logger 的 Unity Console Logger
        /// </summary>
        private ILogger UnityConsoleLogger { get; set; }

        /// <inheritdoc />
        public override void OnInit()
        {
            bool isHandleUnityDebug = !Application.isEditor || this.useCysConsoleLogger;

            UnityConsoleLogger = isHandleUnityDebug
                ? new UnityEngine.Logger(Debug.unityLogger.logHandler)
                : Debug.unityLogger;

            LoggerFactor = new LoggerFactor(CreateLoggerProviders(), UnityConsoleLogger);

            if (isHandleUnityDebug)
            {
                var logger = LoggerFactor.GetOrCreateLogger("UnityDebugLog");
                Debug.unityLogger.logHandler = new UnityLoggerHandler(logger);
            }

            Debug.Log($"Init global loggers, use Cys console logger={isHandleUnityDebug}");
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
        }

        private IEnumerable<ILoggerProvider> CreateLoggerProviders()
        {
            return new ILoggerProvider[]
            {
                new UnityDebugLoggerProvider(UnityConsoleLogger)
            };
        }
    }
}
