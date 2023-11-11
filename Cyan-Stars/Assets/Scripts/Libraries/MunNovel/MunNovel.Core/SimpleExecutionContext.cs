using System;
using MunNovel.Logging;

namespace MunNovel
{
    internal class SimpleExecutionContext : IExecutionContext, IDisposable
    {
        private bool _isDisposed;

        public IServiceProvider Services { get; private set; }
        public ILogger Logger { get; private set; }

        internal SimpleExecutionContext(IServiceProvider services, ILogger logger)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            this.Logger = logger;
            this.Services = services;
        }

        internal SimpleExecutionContext(Func<IExecutionContext, IServiceProvider> servicesProviderFactor, ILogger logger)
        {
            _ = servicesProviderFactor ?? throw new ArgumentNullException(nameof(servicesProviderFactor));

            this.Logger = logger;
            Services = servicesProviderFactor(this) ?? throw new InvalidOperationException("Create service provider failed");
        }

        protected void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                (Services as IDisposable)?.Dispose();
                (Logger as IDisposable)?.Dispose();
            }

            Services = null;
            Logger = null;

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}