using System;
using System.Collections.Generic;
using System.Text;

namespace MunNovel.Executor
{
    public class ScriptExecuteException : Exception
    {
        public ScriptExecuteException()
        {

        }

        public ScriptExecuteException(string message) : base(message)
        {

        }
    }
}
