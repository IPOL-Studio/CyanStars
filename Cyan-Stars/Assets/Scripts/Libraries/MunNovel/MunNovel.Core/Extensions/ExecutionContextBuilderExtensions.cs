using System;
using MunNovel.Logging;

namespace MunNovel
{
    public static class ExecutionContextBuilderExtensions
    {
        public static IExecutionContextBuilder ConfigureLogger(this IExecutionContextBuilder builder, ILogger logger)
        {
            builder.Logger = () => logger;
            return builder;
        }

        public static IExecutionContextBuilder ConfigureLogger(this IExecutionContextBuilder builder, Func<ILogger> func)
        {
            builder.Logger = func;
            return builder;
        }
    }
}