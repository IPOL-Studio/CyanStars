using System;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public class ScriptContext : IScriptContext
    {
        private IScriptExecutorCommandBuffer _buffer;
        public IExecutionContext ExecutionContext { get; }

        public ScriptContext(IScriptExecutorCommandBuffer buffer, IExecutionContext executionContext)
        {
            _buffer = buffer;
            ExecutionContext = executionContext;
        }

        public IScriptContext Execute(ICommand command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            _buffer.AddCommand(command);
            return this;
        }

        public async ValueTask Submit()
        {
            await _buffer.Execute();
        }

        public ValueTask Pause(double time = -1)
        {
            return _buffer.Pause(time);
        }
    }
}