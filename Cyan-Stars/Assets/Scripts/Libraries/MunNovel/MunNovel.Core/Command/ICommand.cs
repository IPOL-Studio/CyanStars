using System.Threading;
using System.Threading.Tasks;

namespace MunNovel.Command
{
    public interface ICommand
    {
        ValueTask ExecuteAsync(IExecutionContext ctx, CancellationToken cancellationToken = default);
    }
}