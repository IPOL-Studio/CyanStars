using System.Collections;
using System.Collections.Generic;

namespace MunNovel.Command
{
    public sealed class DictionaryParameterProvider : ICommandParameterProvider
    {
        private IReadOnlyDictionary<string, object> _parameters;
        public int Count => _parameters.Count;

        public DictionaryParameterProvider(IReadOnlyDictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetValue(string name, out object value) => _parameters.TryGetValue(name, out value);

        public bool TryGetValue<T>(string name, out T value)
        {
            if (_parameters.TryGetValue(name, out var obj) && obj is T)
            {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }
    }
}