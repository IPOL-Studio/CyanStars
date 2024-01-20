using System.Runtime.CompilerServices;

namespace CyanStars.Framework.Logging
{
    public static class LoggerManagerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICysLogger GetOrCreateLogger(this LoggerManager self, string categoryName) =>
            self.LoggerFactor.GetOrCreateLogger(categoryName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICysLogger GetOrCreateLogger<TCategoryName>(this LoggerManager self) =>
            self.GetOrCreateLogger(typeof(TCategoryName).FullName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLogger(this LoggerManager self, string categoryName, out ICysLogger logger) =>
            self.LoggerFactor.TryGetLogger(categoryName, out logger);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetLogger<TCategoryName>(this LoggerManager self, out ICysLogger logger) =>
            self.TryGetLogger(typeof(TCategoryName).FullName, out logger);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveLogger(this LoggerManager self, string categoryName) =>
            self.LoggerFactor.RemoveLogger(categoryName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveLogger<T>(this LoggerManager self) =>
            self.RemoveLogger(typeof(T).FullName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICysLogger GetLogger(this LoggerManager self, string categoryName) =>
            self.LoggerFactor.GetLogger(categoryName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICysLogger GetLogger<T>(this LoggerManager self) =>
            self.GetLogger(typeof(T).FullName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetUnityDebugLogger(this LoggerManager self, out ICysLogger logger) =>
            self.TryGetLogger(LoggerManager.UnityDebugToCysLoggerName, out logger);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICysLogger GetUnityDebugLogger(this LoggerManager self) =>
            self.GetLogger(LoggerManager.UnityDebugToCysLoggerName);
    }
}
