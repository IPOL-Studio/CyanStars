namespace MunNovel.Service
{
    public interface IServiceRegisterHandler
    {
        void OnRegistered(IExecutionContext ctx);
        void OnUnregister(IExecutionContext ctx);
    }
}