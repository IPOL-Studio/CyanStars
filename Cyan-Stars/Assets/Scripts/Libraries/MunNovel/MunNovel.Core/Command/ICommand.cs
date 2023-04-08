using System.Threading;
using System.Threading.Tasks;

namespace MunNovel.Command
{
    public interface ICommand
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}