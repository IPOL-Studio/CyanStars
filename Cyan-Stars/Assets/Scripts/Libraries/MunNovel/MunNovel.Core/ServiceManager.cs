using System;
using System.Collections.Generic;
using MunNovel.Service;

namespace MunNovel
{
    public sealed class ServiceManager
    {
        private static Func<ServiceManager> _getInstanceFunc;
        public static ServiceManager Instance => _getInstanceFunc?.Invoke();

        private readonly Dictionary<Type, IService> ServiceDict = new Dictionary<Type, IService>();

        public static ICommandManager CommandManager { get; private set; }

        public void Register<T>(T service) where T : class, IService
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Unregister<T>();
            ServiceDict[typeof(T)] = service;
            service.OnRegister();
        }

        public bool Unregister<T>() where T : class, IService
        {
            if (!ServiceDict.TryGetValue(typeof(T), out IService service))
            {
                return false;
            }

            try
            {
                service.OnUnregister();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                ServiceDict.Remove(typeof(T));
            }

            return true;
        }

        public T GetService<T>() where T : class, IService => (T)ServiceDict.GetValueOrDefault(typeof(T));

        public bool TryGetService<T>(out T service) where T : class, IService
        {
            bool isExistSerivce = ServiceDict.TryGetValue(typeof(T), out var s);
            service = (T)s;
            return isExistSerivce;
        }

        public bool IsExists<T>() where T : class, IService => ServiceDict.ContainsKey(typeof(T));

        public static void SetInstanceProvider(Func<ServiceManager> provider) => _getInstanceFunc = provider;

        public void Register(ICommandManager manager)
        {
            CommandManager = manager;
        }
    }
}
