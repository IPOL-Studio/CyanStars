namespace MunNovel.Storage
{
    public interface IVariableScope
    {
        void SetData(string name, object data);
        bool TryGetData(string name, out object result);
        bool RemoveData(string name);
        void Clear();
    }
}
