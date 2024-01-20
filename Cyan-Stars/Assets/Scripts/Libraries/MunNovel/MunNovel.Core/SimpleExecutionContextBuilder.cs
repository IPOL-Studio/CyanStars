using System;
using System.Collections;
using System.Collections.Generic;
using MunNovel.Logging;
using MunNovel.Service;

namespace MunNovel
{
    public sealed partial class SimpleExecutionContextBuilder : IExecutionContextBuilder
    {
        public IServiceCollection Services { get; }
        public Func<ILogger> Logger { get; set; }

        public SimpleExecutionContextBuilder()
        {
            this.Services = new ServiceCollection();
        }

        public IExecutionContext Build()
        {
            var logger = Logger?.Invoke();

            SimpleServiceProvider provider = null;

            var ctx = new SimpleExecutionContext(ctx => provider = CreateServiceProvider(Services, ctx), logger);
            InitServices(provider, Services);
            return ctx;
        }

        private static SimpleServiceProvider CreateServiceProvider(IServiceCollection services, IExecutionContext ctx)
        {
            return new SimpleServiceProvider(
                services,
                (type, creator) => creator.Create(ctx),
                onServiceCreated: service => (service as IServiceRegisterHandler)?.OnRegistered(ctx),
                onServiceDestroy: service => (service as IServiceRegisterHandler)?.OnUnregister(ctx));
        }

        private static void InitServices(IServiceProvider provider, IServiceCollection descriptions)
        {
            foreach (var desc in descriptions)
            {
                if (desc.CreateTiming == ServiceCreateTiming.Immediate)
                {
                    provider.GetService(desc.ServiceType);
                }
            }

            foreach (var desc in descriptions)
            {
                if (desc.CreateTiming == ServiceCreateTiming.AfterImmediate)
                {
                    provider.GetService(desc.ServiceType);
                }
            }
        }
    }

    partial class SimpleExecutionContextBuilder
    {
        private class ServiceCollection : IServiceCollection
        {
            private List<ServiceDescriptor> _list = new List<ServiceDescriptor>();

            public ServiceDescriptor this[int index]
            {
                get => _list[index];
                set => throw new NotSupportedException();
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            public void Add(ServiceDescriptor item)
            {
                _list.Add(item);
            }

            public void Clear()
            {
                _list.Clear();
            }

            public bool Contains(ServiceDescriptor item)
            {
                if (item.ServiceType == null)
                    return false;

                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].ServiceType == item.ServiceType)
                        return true;
                }

                return false;
            }

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => throw new NotSupportedException();

            public IEnumerator<ServiceDescriptor> GetEnumerator() => _list.GetEnumerator();

            public int IndexOf(ServiceDescriptor item) => _list.IndexOf(item);

            public void Insert(int index, ServiceDescriptor item) => throw new NotSupportedException();

            public bool Remove(ServiceDescriptor item)
            {
                if (item.ServiceType == null)
                    return false;

                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].ServiceType == item.ServiceType)
                    {
                        _list.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public void RemoveAt(int index) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _list.GetEnumerator();
            }
        }
    }
}