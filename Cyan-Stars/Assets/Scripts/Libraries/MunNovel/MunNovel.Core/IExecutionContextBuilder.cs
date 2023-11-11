using System;
using MunNovel.Logging;

namespace MunNovel
{
    public interface IExecutionContextBuilder
    {
        IServiceCollection Services { get; }
        Func<ILogger> Logger { get; set; }

        IExecutionContext Build();
    }
}