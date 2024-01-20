using System;

namespace MunNovel
{
    public readonly struct ServiceDescriptor
    {
        public readonly Type ServiceType;
        public readonly IServiceCreator Creator;
        public readonly ServiceCreateTiming CreateTiming;

        public ServiceDescriptor(Type serviceType, IServiceCreator creator, ServiceCreateTiming createTiming)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Creator = creator;
            CreateTiming = createTiming;
        }
    }
}