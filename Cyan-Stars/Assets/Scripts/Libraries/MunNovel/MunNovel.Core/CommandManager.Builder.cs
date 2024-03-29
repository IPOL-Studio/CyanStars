using System;
using System.Collections.Generic;
using System.Reflection;
using MunNovel.Metadata;
using MunNovel.Utils;

namespace MunNovel
{
    public partial class CommandManager
    {
        public sealed class Builder
        {
            public CommandManager Build()
            {
                var commands = CollectCommands();
                var manager = new CommandManager();

                foreach (var cmd in commands)
                {
                    manager.AddCommandMetadata(cmd);
                    foreach (var name in cmd.CommandNames)
                    {
                        manager.RegisterCommand(name, cmd);
                    }
                }

                return manager;
            }

            private List<CommandMetadata> CollectCommands()
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var commands = new List<CommandMetadata>();

                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!CanCreateCommand(type))
                            continue;

                        var metadata = CommandMetadata.CreateMetadata(type);
                        commands.Add(metadata);
                    }
                }

                return commands;
            }

            private bool CanCreateCommand(Type type)
            {
                if (type.IsAbstract || type.IsInterface || type.IsNested || !type.IsClass || !type.IsPublic || !CommandUtils.IsCommand(type))
                    return false;

                var creatorAttr = type.GetCustomAttribute<CustomCommandCreatorAttribute>();
                if (creatorAttr != null && creatorAttr.IsValid)
                    return true;

                var ctors = type.GetConstructors();
                foreach (var ctor in ctors)
                {
                    if (ctor.IsPublic)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
