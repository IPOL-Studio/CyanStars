using MunNovel.Storage;

namespace MunNovel.Service
{
    public interface IVariableStorage
    {
        IVariableScope CreateScope(string name);
        bool RemoveScope(string name);
        bool TryGetScope(string name, out IVariableScope scope);
    }
}