using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public interface IScriptExecutorCommandBuffer
    {
        bool CanExecute { get; }

        void AddCommand(ICommand command);
        ValueTask Execute();
        ValueTask Pause(double time = -1);
    }
}