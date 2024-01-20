using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MunNovel.Command;
using MunNovel.Utils;

namespace MunNovel.Metadata
{
    public struct CommandMetadataBuilder
    {
        public Type CommandType { get; }
        public IReadOnlyList<string> CommandNames;
        public Dictionary<string, CommandParameterMetadata> Parameters;
        public ICommandCreator CustomCreator;

        private CommandMetadataBuilder(Type commandType) : this()
        {
            CommandType = commandType;
        }

        public CommandMetadata Build()
        {
            return CommandMetadata.CreateMetadata(CommandType, CommandNames, Parameters, CustomCreator);
        }

        public static CommandMetadataBuilder Create(Type commandType)
        {
            if (!CommandUtils.IsCommand(commandType))
                throw new ArgumentException($"{commandType} is not a command type", nameof(commandType));

            return new CommandMetadataBuilder(commandType);
        }

        public static CommandMetadataBuilder Create<T>() where T : ICommand
        {
            return new CommandMetadataBuilder(typeof(T));
        }
    }

    public static class CommandMetadataBuilderExtension
    {
        public static ref CommandMetadataBuilder AddParameter(ref this CommandMetadataBuilder builder, CommandParameterMetadata metadata)
        {
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));

            if (builder.Parameters == null)
                builder.Parameters = new Dictionary<string, CommandParameterMetadata>();

            ThrowExceptionIfParameterDuplicate(ref builder, metadata.Name);
            builder.Parameters.Add(metadata.Name, metadata);

            return ref builder;
        }

        public static ref CommandMetadataBuilder SetNames(ref this CommandMetadataBuilder builder, IReadOnlyList<string> values)
        {
            builder.CommandNames = values;
            return ref builder;
        }

        public static ref CommandMetadataBuilder SetCustomCreator(ref this CommandMetadataBuilder builder, ICommandCreator creator)
        {
            builder.CustomCreator = creator;
            return ref builder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowExceptionIfParameterDuplicate(ref CommandMetadataBuilder builder, string parameterName)
        {
            if (builder.Parameters.ContainsKey(parameterName))
            {
                throw new CommandParameterException(
                    builder.CommandType,
                    parameterName,
                    $"parameter {parameterName} is duplicate in command {builder.CommandType.FullName}"
                );
            }
        }
    }
}