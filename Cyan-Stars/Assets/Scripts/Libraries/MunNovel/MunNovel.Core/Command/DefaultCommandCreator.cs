using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MunNovel.Service;
using MunNovel.Metadata;

namespace MunNovel.Command
{
    public sealed class DefaultCommandCreator : ICommandCreator
    {
        public ICommand Create<T>(ICommandMetadataProvider metadataProvider, Type commandType, ref T parameters) where T : ICommandParameterProvider
        {
            var metadata = metadataProvider.GetCommandMetadata(commandType);
            _ = metadata ?? throw new ArgumentException($"command {commandType} is not register");

            if (!(metadata.CustomCreator is null))
            {
                return metadata.CustomCreator.Create(metadataProvider, commandType, ref parameters);
            }

            if (!metadata.HasEmptyConstructor)
            {
                throw new ArgumentException($"command {commandType} can not be construct");
            }

            return Create(commandType, metadata, ref parameters);
        }

        private static ICommand Create<T>(Type commandType, CommandMetadata metadata, ref T parameters) where T : ICommandParameterProvider
        {
            var command = (ICommand)Activator.CreateInstance(commandType);

            if (parameters is null || parameters.Count == 0)
            {
                return command;
            }

            foreach (var (name, value) in parameters)
            {
                if (metadata.CommandParameters.TryGetValue(name, out var parameter) && IsAssignable(parameter.Type, value))
                {
                    parameter.SetValue(command, value);
                }
            }

            return command;
        }

        private static bool IsAssignable(Type target, object value)
        {
            if (value is null)
            {
                return !target.IsValueType || IsNullable(target);
            }

            return target.IsAssignableFrom(value.GetType());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}