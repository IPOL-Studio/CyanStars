using System;

namespace MunNovel
{
    public static class ServiceCollectionExtensions
    {
        public static void AddService<TService, TImpl>(this IServiceCollection collection,
                                                       ServiceCreateTiming createTiming = ServiceCreateTiming.Immediate)
            where TService : class
            where TImpl : class, TService, new()
        {
            collection.Add(new ServiceDescriptor(typeof(TService), new DirectServiceCreator<TImpl>(), createTiming));
        }

        public static void AddService<TService>(this IServiceCollection collection,
                                                ServiceCreateTiming createTiming = ServiceCreateTiming.Immediate)
            where TService : class, new()
        {
            collection.Add(new ServiceDescriptor(typeof(TService), new DirectServiceCreator<TService>(), createTiming));
        }

        public static void AddService<TService>(this IServiceCollection collection,
                                                Func<TService> func,
                                                ServiceCreateTiming createTiming = ServiceCreateTiming.Immediate)
            where TService : class
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            collection.Add(new ServiceDescriptor(typeof(TService), new FuncServiceCreator<TService>(func), createTiming));
        }

        public static void AddService<TService>(this IServiceCollection collection,
                                                Func<IExecutionContext, TService> func,
                                                ServiceCreateTiming createTiming = ServiceCreateTiming.AfterImmediate)
            where TService : class
        {
            _ = func ?? throw new ArgumentNullException(nameof(func));
            collection.Add(new ServiceDescriptor(typeof(TService), new FuncServiceCreator<TService>(func), createTiming));
        }

        public static void RemoveService<TService>(this IServiceCollection collection)
            where TService : class
        {
            collection.Remove(new ServiceDescriptor(typeof(TService), null, ServiceCreateTiming.Immediate));
        }
    }
}