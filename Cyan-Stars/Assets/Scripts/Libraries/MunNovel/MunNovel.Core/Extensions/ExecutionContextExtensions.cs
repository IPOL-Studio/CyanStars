using System.Runtime.CompilerServices;

namespace MunNovel
{
    public static class ExecutionContextExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetService<T>(this IExecutionContext ctx) where T : class =>
            ctx.Services.GetService<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetService<T>(this IExecutionContext ctx, out T service) where T : class =>
            ctx.Services.TryGetService(out service);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetRequiredService<T>(this IExecutionContext ctx) where T : class =>
            ctx.Services.GetRequiredService<T>();
    }
}