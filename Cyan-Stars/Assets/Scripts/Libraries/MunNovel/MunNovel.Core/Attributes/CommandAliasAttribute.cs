using System;
using System.Collections.Generic;
using System.Text;
using MunNovel.Utils;

namespace MunNovel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandAliasAttribute : Attribute
    {
        private static readonly string InvalidMessage = $"Command name can't contains {CommandUtils.InvalidChars}";

        public readonly string Value;

        public CommandAliasAttribute(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value), "Command name can't be Null or white space");
            }

            if (CommandUtils.ContainsInvalidAliasChar(value))
            {
                throw new ArgumentException(nameof(value), InvalidMessage);
            }

            Value = value;
        }
    }
}