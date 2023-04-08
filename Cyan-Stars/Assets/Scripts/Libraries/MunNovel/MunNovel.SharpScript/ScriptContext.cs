using System;
using System.Threading.Tasks;
using MunNovel.Command;

namespace MunNovel.SharpScript
{
    public class ScriptContext : IScriptContext
    {
        private IScriptExecutorCommandBuffer _buffer;

        public ScriptContext(IScriptExecutorCommandBuffer buffer)
        {
            _buffer = buffer;
        }

        public IScriptContext Execute(ICommand command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            _buffer.AddCommand(command);
            return this;
        }

        public async Task Submit()
        {
            await _buffer.Execute();
        }

        public Task Pause(double time = -1)
        {
            return _buffer.Pause(time);
        }
    }
}
