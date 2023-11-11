using System;

namespace MunNovel.Metadata
{
    public class CommandParameterException : Exception
    {
        public Type CommandType { get; }
        public string ParameterName { get; }

        public CommandParameterException(Type commandType, string parameterName)
            : this(commandType, parameterName, null)
        {
        }

        public CommandParameterException(Type commandType, string parameterName, string message) : base(message)
        {
            CommandType = commandType;
            ParameterName = parameterName;
        }
    }
}