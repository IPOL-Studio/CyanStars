using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public interface IScriptContext
    {
        IScriptContext Execute(ICommand command);
        Task Submit();
        Task Pause(double time = -1);
    }
}
