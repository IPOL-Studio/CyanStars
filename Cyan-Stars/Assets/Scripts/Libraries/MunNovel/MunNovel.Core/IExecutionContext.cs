using System;
using MunNovel.Logging;

namespace MunNovel
{
    public interface IExecutionContext
    {
        IServiceProvider Services { get; }
        ILogger Logger { get; }
    }
}