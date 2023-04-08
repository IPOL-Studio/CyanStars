using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MunNovel.Command;
using MunNovel.SharpScript;

namespace CyanStars.Framework.Dialogue
{
    public partial class UnitySharpScriptExecutor
    {
        private class CommandBuffer : IScriptExecutorCommandBuffer
        {
            private UnitySharpScriptExecutor executor;
            private bool isExecuting;

            private List<ICommand> executingCommands = new List<ICommand>(10);
            private Queue<ICommand> waitExecuteCommands = new Queue<ICommand>(10);

            public bool CanExecute => !isExecuting && waitExecuteCommands.Count > 0;

            public CommandBuffer(UnitySharpScriptExecutor executor)
            {
                this.executor = executor;
            }

            public void AddCommand(ICommand command)
            {
                _ = command ?? throw new ArgumentNullException(nameof(command));
                waitExecuteCommands.Enqueue(command);
            }

            public async Task Execute()
            {
                if (!CanExecute)
                    return;

                isExecuting = true;

                while (waitExecuteCommands.Count > 0)
                {
                    await Execute(executor.ExecutingTasks);
                }

                isExecuting = false;
            }

            private async Task Execute(List<Task> executing)
            {
                double? pauseTime = null;
                CancellationToken cancelToken = executor.CommandCancelToken;

                while (true)
                {
                    ICommand cmd = waitExecuteCommands.Dequeue();
                    executingCommands.Add(cmd);
                    executing.Add(cmd.ExecuteAsync(cancelToken));

                    bool isPause = typeof(IPauseableCommand).IsAssignableFrom(cmd.GetType());

                    if (waitExecuteCommands.Count == 0 || isPause)
                    {
                        if (isPause)
                            pauseTime = ((IPauseableCommand)cmd).PauseTime;

                        break;
                    }
                }

                await Task.WhenAll(executing);

                executor.CommandsExecuted(executingCommands);
                executing.Clear();

                if (pauseTime.HasValue)
                    await Pause(pauseTime.Value);
            }

            public Task Pause(double time = -1d)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                executor.Pause(tcs, time);
                return tcs.Task;
            }
        }
    }
}
