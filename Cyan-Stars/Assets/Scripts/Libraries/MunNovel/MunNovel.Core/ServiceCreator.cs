using System;

namespace MunNovel
{
    internal sealed class DirectServiceCreator<T> : IServiceCreator where T : class, new()
    {
        public object Create(IExecutionContext ctx) => new T();
    }

    internal sealed class FuncServiceCreator<T> : IServiceCreator where T : class
    {
        private object _func;
        private bool _isWithCtx;

        public FuncServiceCreator(Func<IExecutionContext, T> func)
        {
            _func = func;
            _isWithCtx = true;
        }

        public FuncServiceCreator(Func<T> func)
        {
            _func = func;
            _isWithCtx = false;
        }

        public object Create(IExecutionContext ctx)
        {
            return _isWithCtx
                ? ((Func<IExecutionContext, T>)_func)(ctx)
                : ((Func<T>)_func)();
        }
    }
}