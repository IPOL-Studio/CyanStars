using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MunNovel.Command
{
    internal sealed class SharedCommandCreators
    {
        private Dictionary<Type, ICommandCreator> _creators;
        private Dictionary<Type, ICommandCreator> Creators
        {
            get
            {
                if (_creators == null)
                    _creators = new Dictionary<Type, ICommandCreator>();

                return _creators;
            }
        }

        public ICommandCreator GetOrCreate(Type creatorType)
        {
            return GetCreator(creatorType)
                ?? AddCreator(creatorType, (ICommandCreator)Activator.CreateInstance(creatorType));
        }

        public ICommandCreator GetOrCreate<T>() where T : ICommandCreator, new()
        {
            return GetCreator(typeof(T)) ?? AddCreator(typeof(T), new T());
        }

        /// <returns>creator or null</returns>
        public ICommandCreator GetCreator(Type creatorType)
        {
            return _creators?.GetValueOrDefault(creatorType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ICommandCreator AddCreator(Type type, ICommandCreator creator)
        {
            Creators.Add(type, creator);
            return creator;
        }
    }
}