using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public interface IScriptExecutorCommandBuffer
    {
        bool CanExecute { get; }

        void AddCommand(ICommand command);

        Task Execute();

        Task Pause(double time = -1);
    }
}
