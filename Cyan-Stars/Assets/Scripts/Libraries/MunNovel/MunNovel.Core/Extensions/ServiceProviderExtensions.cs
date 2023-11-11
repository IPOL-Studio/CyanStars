using System;

namespace MunNovel
{
    public static class ServiceProviderExtensions
    {
        /// <returns>null or service instance</returns>
        public static T GetService<T>(this IServiceProvider self) where T : class
        {
            return self.GetService(typeof(T)) as T;
        }

        public static bool TryGetService<T>(this IServiceProvider self, out T service) where T : class
        {
            var s = self.GetService(typeof(T));
            service = s as T;
            return service != null;
        }

        public static T GetRequiredService<T>(this IServiceProvider self) where T : class
        {
            var service = self.GetService<T>()
                ?? throw new InvalidOperationException($"Service {typeof(T).FullName} is not registered.");
            return service;
        }
    }
}
