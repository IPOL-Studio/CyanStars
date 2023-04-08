using System;
using MunNovel.Command;

namespace MunNovel.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class CustomCommandCreatorAttribute : Attribute
    {
        private Type _creatorType;
        public readonly bool IsLazy;
        public readonly bool IsValid;

        private ICommandCreator _creator;
        public ICommandCreator Creator
        {
            get
            {
                if (IsValid && _creator is null)
                {
                    CreateCommandCreator();
                }

                return _creator;
            }
        }

        public CustomCommandCreatorAttribute(Type creatorType, bool isLazy = false)
        {
            _creatorType = creatorType;
            IsLazy = isLazy;
            IsValid = creatorType != null &&
                      typeof(ICommandCreator).IsAssignableFrom(creatorType) &&
                      (creatorType.GetConstructor(Type.EmptyTypes)?.IsPublic ?? false);

            if (IsValid && !isLazy)
            {
                CreateCommandCreator();
            }
        }

        private void CreateCommandCreator()
        {
            if (_creator is null)
            {
                _creator = (ICommandCreator)Activator.CreateInstance(_creatorType);
            }
        }
    }
}