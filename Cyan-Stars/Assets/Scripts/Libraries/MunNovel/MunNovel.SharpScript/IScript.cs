using System.Threading;
using System.Threading.Tasks;

namespace MunNovel.SharpScript
{
    public interface IScript
    {
        Task PreLoad(CancellationToken cancellationToken = default);
        Task Execute(IScriptContext context);
    }
}
