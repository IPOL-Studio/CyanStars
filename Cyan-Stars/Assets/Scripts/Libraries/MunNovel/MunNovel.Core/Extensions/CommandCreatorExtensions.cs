using System;
using MunNovel.Service;

namespace MunNovel.Command
{
    public static class CommandCreatorExtensions
    {
        public static ICommand Create(this ICommandCreator creator, ICommandMetadataProvider metadataProvider, Type commandType)
        {
            ICommandParameterProvider parameters = null;
            return creator.Create(metadataProvider, commandType, ref parameters);
        }
    }
}