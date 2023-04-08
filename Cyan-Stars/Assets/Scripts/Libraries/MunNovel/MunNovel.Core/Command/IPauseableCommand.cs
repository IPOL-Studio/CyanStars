using System;
using System.Collections.Generic;
using System.Text;

namespace MunNovel.Command
{
    public interface IPauseableCommand
    {
        bool IsPause { get; }
        double PauseTime { get; }
    }
}
