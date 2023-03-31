using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.Service
{
    public class CommandFactorImpl : ICommandFactor
    {
        private readonly ICommandCreator DefaultCreator;

        public CommandFactorImpl(ICommandCreator defaultCreator = null)
        {
            DefaultCreator = defaultCreator ?? new DefaultCommandCreator();
        }

        public ICommand CreateCommand(Type commandType, Dictionary<string, object> param)
        {
            return CreateCommand(ServiceManager.CommandManager.GetCommandMetadata(commandType), param);
        }

        public ICommand CreateCommand(string commandName, Dictionary<string, object> param)
        {
            return CreateCommand(ServiceManager.CommandManager.GetCommandMetadata(commandName), param);
        }

        private ICommand CreateCommand(CommandMetadata metadata, Dictionary<string, object> param)
        {
            if (metadata == null)
                return null;

            var creator = metadata.CustomCreator ?? DefaultCreator;
            return creator.Create(metadata.CommandType, param);
        }

        public void OnRegister()
        {
        }

        public void OnUnregister()
        {
        }
    }
}
