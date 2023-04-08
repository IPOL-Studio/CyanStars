using System;
using MunNovel.Utils;

namespace MunNovel.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CommandParameterAttribute : Attribute
    {
        private static readonly string InvalidMessage = $"Command parameter name can't contains {CommandUtils.InvalidChars}";

        public readonly string Value;

        public CommandParameterAttribute(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value), "Command parameter name can't be Null or white space");
            }

            if (CommandUtils.ContainsInvalidAliasChar(value))
            {
                throw new ArgumentException(nameof(value), InvalidMessage);
            }

            Value = value;
        }
    }
}