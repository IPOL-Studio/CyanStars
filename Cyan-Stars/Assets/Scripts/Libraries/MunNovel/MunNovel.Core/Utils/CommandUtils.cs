using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MunNovel.Command;
using MunNovel.Metadata;

namespace MunNovel.Utils
{
    public static partial class CommandUtils
    {
        public static string[] GetCommandNames(Type commandType)
        {
            _ = commandType ?? throw new ArgumentNullException(nameof(commandType));

            if (!IsCommand(commandType))
            {
                throw new ArgumentException($"{commandType.FullName} not assignable from ICommand", nameof(commandType));
            }

            var alias = GetAlias(commandType);
            return alias == null || alias == commandType.FullName
                ? new string[] { commandType.FullName }
                : new string[]
                {
                        commandType.FullName,
                        alias
                };
        }

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal static void FillCommandParameters(ref CommandMetadataBuilder builder)
        {
            var commandType = builder.CommandType ?? throw new ArgumentNullException(nameof(builder.CommandType));

            var props = commandType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty);
            AddPropertyCommandParameters(ref builder, props);

            var fields = commandType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);
            AddFieldCommandParameters(ref builder, fields);
        }

        // 写入以 property 形式声明的 parameters
        // property 需要具有 set 访问器(可写)
        // property 需要 [CommandParameterAttribute]，或 set 为 public
        private static void AddPropertyCommandParameters(ref CommandMetadataBuilder builder, PropertyInfo[] props)
        {
            for (int i = 0; i < props.Length; i++)
            {
                // 属性不可写，跳过
                if (!props[i].CanWrite)
                    continue;

                // 没有CommandParameterAttribute做标记，并且set不是public的，跳过
                if (!props[i].HasCommandParameterAttribute() && props[i].GetSetMethod(false) == null)
                    continue;

                builder.AddParameter(CommandParameterMetadata.Create(props[i]));
            }
        }

        // 写入以 field 形式声明的 parameters
        // field 需要 [CommandParameterAttribute]，且非 readonly
        private static void AddFieldCommandParameters(ref CommandMetadataBuilder builder, FieldInfo[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].HasCommandParameterAttribute() && !fields[i].IsInitOnly)
                {
                    var metadata = CommandParameterMetadata.Create(fields[i]);
                    builder.AddParameter(metadata);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasCommandParameterAttribute(this MemberInfo member) =>
            member.IsDefined(typeof(CommandParameterAttribute), false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCommand(Type type) => typeof(ICommand).IsAssignableFrom(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasEmptyConstructor(Type type) => type.GetConstructor(Type.EmptyTypes) != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetAlias(Type type) => type.GetCustomAttribute<CommandAliasAttribute>()?.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICommandCreator GetCustomCreator(Type type) =>
            type.GetCustomAttribute<CustomCommandCreatorAttribute>()?.Creator;
    }

    public static partial class CommandUtils
    {
        private static readonly HashSet<char> InvalidCharSet = new HashSet<char>
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
                    foreach (var item in InvalidCharSet)
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
                if (InvalidCharSet.Contains(alias[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}