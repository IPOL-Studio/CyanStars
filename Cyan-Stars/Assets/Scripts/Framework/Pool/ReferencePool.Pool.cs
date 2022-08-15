using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Pool
{
    public static partial class ReferencePool
    {
        private class Pool
        {
            private readonly List<IReference> Refs = new List<IReference>();

            public T Get<T>() where T : IReference, new()
            {
                if (Refs.Count == 0)
                {
                    T obj = new T();
                    //Debug.Log($"创建引用:{obj.GetType()}");
                    return obj;
                }

                IReference reference = Refs[Refs.Count - 1];
                Refs.RemoveAt(Refs.Count - 1);

                //Debug.Log($"获取引用:{typeof(T).Name}");
                return (T) reference;
            }

            public void Release(IReference reference)
            {
                //Debug.Log($"归还引用:{reference.GetType()}");
                reference.Clear();
                Refs.Add(reference);
            }
        }
    }

}
