using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using ParameterMemberTypes = MunNovel.Command.CommandParameterMetadata.ParameterMemberTypes;

namespace MunNovel.Command
{
    public class DefaultCommandCreator : ICommandCreator
    {
        public ICommand Create(Type commandType, Dictionary<string, object> param)
        {
            var metadata = ServiceManager.CommandManager.GetCommandMetadata(commandType);
            if (metadata == null)
            {
                throw new ArgumentException($"command type {commandType} is not register");
            }

            if (metadata.CustomCreator != null)
            {
                return metadata.CustomCreator.Create(commandType, param);
            }
 
            if (!metadata.HasEmptyConstructor)
            {
                throw new ArgumentException($"command type {commandType} can not be construct");
            }

            var command = Activator.CreateInstance(commandType) as ICommand;

            foreach (var inputParam in param)
            {
                if (metadata.CommandParameters.TryGetValue(inputParam.Key, out var prop) && IsAssignable(prop.Type, inputParam.Value))
                {
                    if (prop.MemberType == ParameterMemberTypes.Field)
                    {
                        (prop.Member as FieldInfo).SetValue(command, inputParam.Value);
                    }
                    else if (prop.MemberType == ParameterMemberTypes.Property)
                    {
                        (prop.Member as PropertyInfo).SetValue(command, inputParam.Value);
                    }
                }
            }

            return command;
        }

        private static bool IsAssignable(Type target, object value)
        {
            if (value == null)
            {
                return !target.IsValueType || target == typeof(Nullable<>);
            }

            return target.IsAssignableFrom(value.GetType());
        }
    }
}