using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public interface IScriptContext
    {
        IExecutionContext ExecutionContext { get; }

        IScriptContext Execute(ICommand command);
        ValueTask Submit();
        ValueTask Pause(double time = -1);
    }
}