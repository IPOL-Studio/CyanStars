using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MunNovel.Command;

namespace MunNovel.Service
{
    public static class CommandFactorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICommand CreateCommand<T>(this ICommandFactor self, string commandName, T parameters) where T : class, ICommandParameterProvider =>
            self.CreateCommand(commandName, ref parameters);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICommand CreateCommand<T>(this ICommandFactor self, Type commandType, T parameters) where T : class, ICommandParameterProvider =>
            self.CreateCommand(commandType, ref parameters);

        public static ICommand CreateCommand(this ICommandFactor self, string commandName, IReadOnlyDictionary<string, object> parameters) =>
            self.CreateCommand(commandName, new DictionaryParameterProvider(parameters));

        public static ICommand CreateCommand(this ICommandFactor self, Type commandType, IReadOnlyDictionary<string, object> parameters) =>
            self.CreateCommand(commandType, new DictionaryParameterProvider(parameters));
    }
}