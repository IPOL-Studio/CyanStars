namespace MunNovel
{
    public interface IServiceCreator
    {
        object Create(IExecutionContext ctx);
    }
}