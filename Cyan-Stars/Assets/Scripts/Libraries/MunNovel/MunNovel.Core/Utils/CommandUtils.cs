using System.Collections.ObjectModel;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MunNovel.Attributes;
using MunNovel.Command;
using System.Collections.Generic;
using System.Text;

namespace MunNovel.Utils
{
    public static partial class CommandUtils
    {
        private static readonly ReadOnlyDictionary<string, CommandParameterMetadata> EmptyCommandPropMetadata =
            new ReadOnlyDictionary<string, CommandParameterMetadata>(new Dictionary<string, CommandParameterMetadata>(0));

        public static string[] GetCommandNames(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            if (!IsCommmand(commandType))
            {
                throw new ArgumentException($"{commandType.FullName} not assignable from ICommand", nameof(commandType));
            }

            var alias = GetAlias(commandType);
            return alias == null
                ? new string[] { commandType.FullName }
                : new string[]
                {
                        commandType.FullName,
                        alias
                };
        }

        public static ReadOnlyDictionary<string, CommandParameterMetadata> GetCommandParameters(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            if (!IsCommmand(commandType))
            {
                throw new ArgumentException($"{commandType.FullName} not assignable from ICommand", nameof(commandType));
            }

            var props = commandType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);
            Dictionary<string, CommandParameterMetadata> dict = null;

            if (props != null && props.Length > 0)
            {
                dict = new Dictionary<string, CommandParameterMetadata>(props.Length);
                GetPropertyCommandParameter(props, dict);
            }

            return dict == null
                ? EmptyCommandPropMetadata
                : new ReadOnlyDictionary<string, CommandParameterMetadata>(dict);
        }

        private static void GetPropertyCommandParameter(PropertyInfo[] props, Dictionary<string, CommandParameterMetadata> dict)
        {
            for (int i = 0; i < props.Length; i++)
            {
                // 属性不可写，跳过
                if (!props[i].CanWrite)
                {
                    continue;
                }

                // 没有CommandParameterAttribute做标记，并且set不是public的，跳过
                if (!props[i].IsDefined(typeof(CommandParameterAttribute), false) && 
                    props[i].GetSetMethod(true) == null)
                {
                    continue;
                }

                var metadata = new CommandParameterMetadata(props[i]);
                dict.Add(metadata.Name, metadata);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCommmand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetAlias(Type type)
        {
            var alias = type.GetCustomAttribute<CommandAliasAttribute>();
            return alias?.Value;
        }
    }

    public static partial class CommandUtils
    {
        private static readonly HashSet<char> CantValidSet = new HashSet<char>
        {
            ' ',
            '=',
            ':'
        };

        private static string _invalidChars;
        internal static string InvalidChars
        {
            get
            {
                if (_invalidChars == null)
                {
                    var sb = new StringBuilder();
                    foreach (var item in CantValidSet)
                    {
                        sb.Append('\'').Append(item).Append("\'  ");
                    }

                    _invalidChars = sb.ToString();
                }

                return _invalidChars;
            }
        }

        internal static bool ContainsInvalidAliasChar(string alias)
        {
            if (string.IsNullOrEmpty(alias) || string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            for (int i = 0; i < alias.Length; i++)
            {
                if (CantValidSet.Contains(alias[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}