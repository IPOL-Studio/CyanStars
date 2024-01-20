using System;
using System.Runtime.CompilerServices;
using MunNovel.Command;
using MunNovel.Metadata;

namespace MunNovel.Service
{
    public class DefaultCommandFactor : ICommandFactor, IServiceRegisterHandler
    {
        private readonly ICommandCreator DefaultCreator;
        private IServiceProvider _services;

        private ICommandMetadataProvider MetadataProvider
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _services.GetService<ICommandMetadataProvider>();
        }

        public DefaultCommandFactor()
        {
            DefaultCreator = new DefaultCommandCreator();
        }

        public DefaultCommandFactor(ICommandCreator defaultCreator)
        {
            DefaultCreator = defaultCreator ?? throw new ArgumentNullException(nameof(defaultCreator));
        }

        public ICommand CreateCommand<T>(Type commandType, ref T parameters) where T : ICommandParameterProvider
        {
            return CreateCommand(MetadataProvider.GetCommandMetadata(commandType), ref parameters);
        }

        public ICommand CreateCommand<T>(string commandName, ref T parameters) where T : ICommandParameterProvider
        {
            return CreateCommand(MetadataProvider.GetCommandMetadata(commandName), ref parameters);
        }

        private ICommand CreateCommand<T>(CommandMetadata metadata, ref T parameters) where T : ICommandParameterProvider
        {
            var creator = metadata?.CustomCreator ?? DefaultCreator;
            return creator.Create(MetadataProvider, metadata.CommandType, ref parameters);
        }

        void IServiceRegisterHandler.OnRegistered(IExecutionContext ctx)
        {
            _services = ctx.Services;
        }

        void IServiceRegisterHandler.OnUnregister(IExecutionContext ctx)
        {
            _services = null;
        }
    }
}