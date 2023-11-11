using System;
using MunNovel.Command;

namespace MunNovel.Metadata
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class CustomCommandCreatorAttribute : Attribute
    {
        private Type _creatorType;
        public readonly bool IsLazy;
        public readonly bool UseShared;
        public readonly bool IsValid;

        private ICommandCreator _creator;
        public ICommandCreator Creator => _creator ?? CreateCommandCreator();

        public CustomCommandCreatorAttribute(Type creatorType, bool useShared = false, bool isLazy = false)
        {
            _creatorType = creatorType;
            UseShared = useShared;
            IsLazy = isLazy;
            IsValid = creatorType != null &&
                      typeof(ICommandCreator).IsAssignableFrom(creatorType) &&
                      (creatorType.GetConstructor(Type.EmptyTypes)?.IsPublic ?? false);

            if (!isLazy)
            {
                CreateCommandCreator();
            }
        }

        private ICommandCreator CreateCommandCreator()
        {
            if (IsValid && _creator is null)
            {
                _creator = UseShared
                    ? MunNovelRoot.SharedCommandCreators.GetOrCreate(_creatorType)
                    : (ICommandCreator)Activator.CreateInstance(_creatorType);
            }

            return _creator;
        }
    }
}