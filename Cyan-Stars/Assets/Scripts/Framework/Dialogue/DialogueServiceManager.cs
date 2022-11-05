using System;
using System.Collections.Generic;
using CyanStars.Framework.Utils;

namespace CyanStars.Framework.Dialogue
{
    public class DialogueServiceManager : BaseManager
    {
        private readonly Dictionary<Type, IService> ServiceDict = new Dictionary<Type, IService>();

        public override int Priority { get; }
        public override void OnInit()
        {
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public TService GetService<TService>() where TService : class, IService
        {
            return ServiceDict.GetValueOrDefault(typeof(TService)) as TService;
        }

        public void RegisterOrReplaceService<TService>(TService service) where TService : class, IService
        {
            if (service is null)
                throw new ArgumentNullException(nameof(service));

            ServiceDict.GetValueOrDefault(typeof(TService))?.OnUnregister();
            ServiceDict[typeof(TService)] = service;
            service.OnRegister();
        }

        public void UnregisterService<TService>() where TService : class, IService
        {
            if (ServiceDict.TryGetValue(typeof(TService), out var service))
            {
                ServiceDict.Remove(typeof(TService));
                service.OnUnregister();
            }
        }
    }
}
