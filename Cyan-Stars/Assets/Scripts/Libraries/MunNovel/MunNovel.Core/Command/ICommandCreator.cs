using System;
using MunNovel.Service;

namespace MunNovel.Command
{
    public interface ICommandCreator
    {
        ICommand Create<T>(ICommandMetadataProvider metadataProvider, Type commandType, ref T parameters) where T : ICommandParameterProvider;
    }
}