using System.Threading;
using System.Threading.Tasks;

namespace MunNovel.Executor
{
    public interface IScriptExecutor
    {
        ScriptExecuteState State { get; }

        void Play();
        bool TryCompleteCurrentExecuting();
    }
}
