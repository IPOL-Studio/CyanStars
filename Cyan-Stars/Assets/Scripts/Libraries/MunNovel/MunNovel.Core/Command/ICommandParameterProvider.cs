using System.Collections.Generic;

namespace MunNovel.Command
{
    public interface ICommandParameterProvider : IReadOnlyCollection<KeyValuePair<string, object>>
    {
        bool TryGetValue(string name, out object value);
        bool TryGetValue<T>(string name, out T value);
    }
}