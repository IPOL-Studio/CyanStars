using System;

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
