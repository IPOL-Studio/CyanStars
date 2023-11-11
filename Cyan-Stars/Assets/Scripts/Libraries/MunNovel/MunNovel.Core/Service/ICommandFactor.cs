using System;
using MunNovel.Command;

namespace MunNovel.Service
{
    public interface ICommandFactor
    {
        ICommand CreateCommand<T>(string commandName, ref T parameters) where T : ICommandParameterProvider;

        ICommand CreateCommand<T>(Type commandType, ref T parameters) where T : ICommandParameterProvider;
    }
}