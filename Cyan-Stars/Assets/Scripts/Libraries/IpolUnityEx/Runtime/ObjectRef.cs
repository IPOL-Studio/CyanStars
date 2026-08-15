using UnityEngine;

namespace Ipol.UnityEx
{
    [System.Serializable]
    public class ObjectRef<T> where T : class
    {
        internal const string ValueFieldName = nameof(value);

        [SerializeField] private Object value;

        public T Value => value as T;
    }
}