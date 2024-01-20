using System;
using System.Collections.Generic;

namespace MunNovel
{
    public class SimpleServiceProvider : IServiceProvider, IDisposable
    {
        private sealed class ServiceAccessor
        {
            private ServiceDescriptor _descriptor;

            public object Service { get; private set; }
            public bool IsCreated => Service != null;

            public object GetOrCreate(Func<Type, IServiceCreator, object> factor)
            {
                if (Service == null)
                {
                    Service = factor.Invoke(_descriptor.ServiceType, _descriptor.Creator);
                }

                return Service;
            }

            public ServiceAccessor(ServiceDescriptor descriptor)
            {
                this._descriptor = descriptor;
            }
        }

        private bool _isDisposed;

        private Dictionary<Type, ServiceAccessor> _services = new Dictionary<Type, ServiceAccessor>();
        private Func<Type, IServiceCreator, object> _servicesFactor;
        private Action<object> _onServicesCreated;
        private Action<object> _onServicesDestroy;

        internal SimpleServiceProvider(ICollection<ServiceDescriptor> descriptors,
                                       Func<Type, IServiceCreator, object> servicesFactor,
                                       Action<object> onServiceCreated = null,
                                       Action<object> onServiceDestroy = null)
        {
            _servicesFactor = servicesFactor ?? throw new ArgumentNullException(nameof(servicesFactor));
            _onServicesCreated = onServiceCreated;
            _onServicesDestroy = onServiceDestroy;

            foreach (var desc in descriptors)
            {
                _services.Add(desc.ServiceType, new ServiceAccessor(desc));
            }
        }

        private object GetOrCreateService(Type serviceType)
        {
            var accessor = _services.GetValueOrDefault(serviceType);
            return accessor == null ? null : GetOrCreateService(accessor);
        }

        private object GetOrCreateService(ServiceAccessor accessor)
        {
            bool isCreate = !accessor.IsCreated;
            object service = accessor.GetOrCreate(_servicesFactor);

            if (isCreate && !(service is null))
            {
                OnServiceCreated(service);
            }

            return service;
        }

        private void OnServiceCreated(object service)
        {
                _onServicesCreated?.Invoke(service);
        }

        private void DestroyAllServices()
        {
            if (_onServicesDestroy is null)
                return;

            foreach (var accessor in _services.Values)
            {
                if (accessor.IsCreated)
                {
                    _onServicesDestroy(accessor.Service);
                }
            }

            _services.Clear();
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (_isDisposed)
                throw new InvalidOperationException("Services provider disposed");

            return GetOrCreateService(serviceType);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            DestroyAllServices();
            _servicesFactor = null;
            _onServicesCreated = null;
            _onServicesDestroy = null;

            _isDisposed = true;
        }

        ~SimpleServiceProvider()
        {
            Dispose(disposing: false);
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}