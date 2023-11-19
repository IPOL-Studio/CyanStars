using System.Runtime.CompilerServices;

namespace CyanStars.Framework.Logging
{
    [System.Flags]
    public enum HideStackTraceFlags
    {
        None = 0,
        AggressiveInlining = 1,
        HideInStackTrace = 2,
        NotFoundFile = 4,
        FoundFile = 8
    }

    public static class HideStackTraceFlagsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagValue(this HideStackTraceFlags flags, HideStackTraceFlags value)
            => (flags & value) == value;
    }
}
