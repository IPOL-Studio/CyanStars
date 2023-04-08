using System;
using MunNovel.Command;

namespace MunNovel
{
    public interface ICommandManager
    {
        CommandMetadata GetCommandMetadata(string commandName);
        CommandMetadata GetCommandMetadata(Type commandType);

        void RegisterCommand(string commandName, CommandMetadata metadata);
        bool UnregisterCommand(string commandName);
        void AddCommandMetadata(CommandMetadata metadata);

        bool IsExistsCommand(string commandName);
        bool IsExistsCommandMetadata(CommandMetadata metadata);
        bool IsExistsCommandMetadata(Type type);

    }

    public static class CommandManagerExtension
    {
        public static void RegisterCommand<T>(this ICommandManager self, string commandName) where T : ICommand
        {
            RegisterCommand(self, commandName, typeof(T));
        }

        public static void RegisterCommand(this ICommandManager self, string commandName, Type commandType)
        {
            var metadata = self.GetCommandMetadata(commandType);
            if (metadata != null)
            {
                self.RegisterCommand(commandName, metadata);
            }
            else
            {
                metadata = CommandMetadata.CreateMetadata(commandType);
                self.AddCommandMetadata(metadata);
                self.RegisterCommand(commandName, metadata);
            }
        }
    
        public static bool IsExistsCommandMetadata<T>(this ICommandManager self) where T : ICommand
        {
            return self.IsExistsCommandMetadata(typeof(T));
        }
    }
}
