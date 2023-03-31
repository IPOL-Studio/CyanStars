using System.Reflection;
using System;
using MunNovel.Attributes;

namespace MunNovel.Command
{
    public sealed class CommandParameterMetadata
    {
        public enum ParameterMemberTypes
        {
            Field,
            Property
        }

        public readonly MemberInfo Member;
        public readonly Type Type;
        public readonly ParameterMemberTypes MemberType;
        public readonly string Name;

        public CommandParameterMetadata(PropertyInfo prop)
        {
            Member = prop;
            Type = prop.PropertyType;
            MemberType = ParameterMemberTypes.Property;
            Name = GetName(prop);
        }

        public CommandParameterMetadata(FieldInfo field)
        {
            Member = field;
            Type = field.FieldType;
            MemberType = ParameterMemberTypes.Field;
            Name = GetName(field);
        }

        private static string GetName(MemberInfo member)
        {
            var attr = member.GetCustomAttribute<CommandParameterAttribute>();
            return attr != null ? attr.Value : member.Name;
        }
    }
}