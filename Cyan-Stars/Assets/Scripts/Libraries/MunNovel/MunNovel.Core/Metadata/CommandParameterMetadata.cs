using System.Reflection;
using System;
using MunNovel.Command;
using System.Runtime.CompilerServices;

namespace MunNovel.Metadata
{
    public abstract partial class CommandParameterMetadata
    {
        public readonly Type Type;
        public readonly string Name;

        protected CommandParameterMetadata(string name, Type type)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name is null or empty", nameof(name));

            Name = name;
            Type = type;
        }

        public abstract void SetValue(ICommand command, object value);
        public abstract object GetValue(ICommand command);
    }

    partial class CommandParameterMetadata
    {
        public static CommandParameterMetadata Create(FieldInfo field) =>
            new AccessorCommandParameterMetadata(GetName(field), field.FieldType, new FieldParameterAccessor(field));

        public static CommandParameterMetadata Create(PropertyInfo property) =>
            new AccessorCommandParameterMetadata(GetName(property), property.PropertyType, new PropertyParameterAccessor(property));

        public static CommandParameterMetadata Create<TC, TP>(string name, ICommandParameterAccessor<TC, TP> accessor)
            where TC : class, ICommand
        {
            return new AccessorCommandParameterMetadata(name, typeof(TP), accessor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetName(MemberInfo member) =>
            member.GetCustomAttribute<CommandParameterAttribute>()?.Value ?? member.Name;
    }

    partial class CommandParameterMetadata
    {
        private sealed class FieldParameterAccessor : ICommandParameterAccessor
        {
            private readonly FieldInfo Target;

            public FieldParameterAccessor(FieldInfo target) => Target = target;
            public void SetValue(ICommand command, object value) => Target.SetValue(command, value);
            public object GetValue(ICommand command) => Target.GetValue(command);
        }

        private sealed class PropertyParameterAccessor : ICommandParameterAccessor
        {
            private readonly PropertyInfo Target;

            public PropertyParameterAccessor(PropertyInfo target) => Target = target;
            public void SetValue(ICommand command, object value) => Target.SetValue(command, value);
            public object GetValue(ICommand command) => Target.GetValue(command);
        }
    }

    internal sealed class AccessorCommandParameterMetadata : CommandParameterMetadata
    {
        private readonly ICommandParameterAccessor Accessor;

        public AccessorCommandParameterMetadata(string name, Type type, ICommandParameterAccessor accessor) : base(name, type)
        {
            _ = accessor ?? throw new ArgumentNullException(nameof(accessor));
            Accessor = accessor;
        }

        public override object GetValue(ICommand command) => Accessor.GetValue(command);
        public override void SetValue(ICommand command, object value) => Accessor.SetValue(command, value);
    }
}