using System;
using System.Collections.Generic;
using MunNovel.Command;
using MunNovel.Utils;

namespace MunNovel.Metadata
{
    public sealed class CommandMetadata
    {
        private static readonly IReadOnlyDictionary<string, CommandParameterMetadata> EmptyCommandParameterMetadata =
            new Dictionary<string, CommandParameterMetadata>(0);

        public Type CommandType { get; }

        public IReadOnlyList<string> CommandNames { get; private set; }
        public IReadOnlyDictionary<string, CommandParameterMetadata> CommandParameters { get; private set; }
        public ICommandCreator CustomCreator { get; }

        public bool HasEmptyConstructor { get; }

        private CommandMetadata(Type commandType,
                                IReadOnlyList<string> commandNames,
                                IReadOnlyDictionary<string, CommandParameterMetadata> commandParameters,
                                bool hasEmptyConstructor,
                                ICommandCreator customCreator)
        {
            CommandType = commandType;
            CommandNames = commandNames;
            CustomCreator = customCreator;
            CommandParameters = commandParameters;
            HasEmptyConstructor = hasEmptyConstructor;
        }

        internal static CommandMetadata CreateMetadata(Type commandType,
                                                       IReadOnlyList<string> commandNames,
                                                       IReadOnlyDictionary<string, CommandParameterMetadata> commandParameters,
                                                       ICommandCreator customCreator)
        {
            _ = commandType ?? throw new ArgumentNullException(nameof(commandType));

            if (!CommandUtils.IsCommand(commandType))
                throw new ArgumentException($"{commandType} is not a command type", nameof(commandType));

            if (commandNames is null || commandNames.Count == 0)
                throw new ArgumentException($"{commandType} command names is null or empty", nameof(commandNames));

            return new CommandMetadata(
                commandType,
                commandNames,
                commandParameters ?? EmptyCommandParameterMetadata,
                CommandUtils.HasEmptyConstructor(commandType),
                customCreator
            );
        }

        public static CommandMetadata CreateMetadata(Type type) =>
            Create(CommandMetadataBuilder.Create(type));

        public static CommandMetadata CreateMetadata<T>() where T : ICommand =>
            Create(CommandMetadataBuilder.Create<T>());

        private static CommandMetadata Create(CommandMetadataBuilder builder)
        {
            var commandType = builder.CommandType;

            builder.SetNames(CommandUtils.GetCommandNames(commandType))
                   .SetCustomCreator(CommandUtils.GetCustomCreator(commandType));

            CommandUtils.FillCommandParameters(ref builder);

            return builder.Build();
        }
    }
}