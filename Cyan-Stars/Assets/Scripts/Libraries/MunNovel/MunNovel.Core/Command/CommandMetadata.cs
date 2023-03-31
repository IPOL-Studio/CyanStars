using System.Collections.ObjectModel;
using System;
using System.Reflection;
using MunNovel.Attributes;
using MunNovel.Collections;
using MunNovel.Utils;

namespace MunNovel.Command
{
    public sealed class CommandMetadata
    {
        public Type CommandType { get; private set; }

        public ReadOnlyArray<string> CommandNames { get; private set; }
        public ReadOnlyDictionary<string, CommandParameterMetadata> CommandParameters { get; private set; }

        private CustomCommandCreatorAttribute _creatorAttr;
        public ICommandCreator CustomCreator => _creatorAttr?.Creator;

        public bool HasEmptyConstructor { get; private set; }

        private CommandMetadata(Type commandType)
        {
            CommandType = commandType;
            CommandNames = CommandUtils.GetCommandNames(commandType).AsReadOnlyArray();
            CommandParameters = CommandUtils.GetCommandParameters(commandType);
            HasEmptyConstructor = commandType.GetConstructor(Type.EmptyTypes) != null;
            _creatorAttr = commandType.GetCustomAttribute<CustomCommandCreatorAttribute>();
        }

        public static CommandMetadata CreateMetadata(Type type)
        {
            if (!CommandUtils.IsCommmand(type))
            {
                throw new ArgumentException($"{type} is not a command type", nameof(type));
            }

            return new CommandMetadata(type);
        }

        public static CommandMetadata CreateMetadata<T>() where T : ICommand
        {
            return new CommandMetadata(typeof(T));
        }
    }
}