using System;
using System.Collections.Generic;
using MunNovel.Metadata;
using MunNovel.Service;

namespace MunNovel
{
    public partial class CommandManager : ICommandMetadataProvider
    {
        private readonly Dictionary<string, CommandMetadata> NameToMetadataDict = new Dictionary<string, CommandMetadata>();
        private readonly Dictionary<Type, CommandMetadata> TypeToMetadataDict = new Dictionary<Type, CommandMetadata>();

        public CommandMetadata GetCommandMetadata(string commandName)
        {
            return NameToMetadataDict.GetValueOrDefault(commandName);
        }

        public CommandMetadata GetCommandMetadata(Type commandType)
        {
            return TypeToMetadataDict.GetValueOrDefault(commandType);
        }

        public void RegisterCommand(string commandName, CommandMetadata metadata)
        {
            if (string.IsNullOrEmpty(commandName) || string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException($"command name \"{commandName}\" invalid", nameof(commandName));
            }

            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));

            if (NameToMetadataDict.ContainsKey(commandName))
            {
                throw new ArgumentException($"command name \"{commandName}\" is contains", nameof(commandName));
            }

            TypeToMetadataDict.TryAdd(metadata.CommandType, metadata);
            NameToMetadataDict.Add(commandName, metadata);
        }

        public bool UnregisterCommand(string commandName)
        {
            return NameToMetadataDict.Remove(commandName);
        }

        public void AddCommandMetadata(CommandMetadata metadata)
        {
            _ = metadata ?? throw new ArgumentNullException(nameof(metadata));
            TypeToMetadataDict.TryAdd(metadata.CommandType, metadata);
        }

        public bool IsExistsCommand(string commandName)
        {
            return !string.IsNullOrEmpty(commandName) &&
                   !string.IsNullOrWhiteSpace(commandName) &&
                   NameToMetadataDict.ContainsKey(commandName);
        }

        public bool IsExistsCommandMetadata(CommandMetadata metadata)
        {
            return metadata != null && TypeToMetadataDict.ContainsKey(metadata.CommandType);
        }

        public bool IsExistsCommandMetadata(Type type)
        {
            return type != null && TypeToMetadataDict.ContainsKey(type);
        }
    }
}
