using System;
using System.Runtime.CompilerServices;
using MunNovel.Command;
using MunNovel.Metadata;

namespace MunNovel.Service
{
    public interface ICommandMetadataProvider
    {
        CommandMetadata GetCommandMetadata(string commandName);
        CommandMetadata GetCommandMetadata(Type commandType);

        bool IsExistsCommand(string commandName);
        bool IsExistsCommandMetadata(CommandMetadata metadata);
        bool IsExistsCommandMetadata(Type commandType);
    }

    public static class CommandMetadataProviderExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsExistsCommandMetadata<T>(this ICommandMetadataProvider self) where T : ICommand
        {
            return self.IsExistsCommandMetadata(typeof(T));
        }
    }
}