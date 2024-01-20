using System.Runtime.CompilerServices;

namespace MunNovel.Command
{
    public static class CommandExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPauseable(this ICommand command) =>
            typeof(IPauseableCommand).IsAssignableFrom(command.GetType());
    }
}