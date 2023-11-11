using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MunNovel.Command;
using MunNovel.SharpScript;
using MunNovel.Utils;

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

            public async ValueTask Execute()
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

            private async ValueTask Execute(List<ValueTask> executing)
            {
                double? pauseTime = null;
                CancellationToken cancelToken = executor.CommandCancelToken;

                while (true)
                {
                    ICommand cmd = waitExecuteCommands.Dequeue();
                    executingCommands.Add(cmd);
                    executing.Add(cmd.ExecuteAsync(executor.executionContext, cancelToken));

                    if (cmd.IsPauseable())
                    {
                        IPauseableCommand pauseCmd = (IPauseableCommand)cmd;
                        if (pauseCmd.IsPause)
                        {
                            pauseTime = pauseCmd.PauseTime;
                        }
                    }

                    if (waitExecuteCommands.Count == 0 || pauseTime.HasValue)
                        break;
                }

                await ValueTaskUtils.WhenAll(executing);

                executor.CommandsExecuted(executingCommands);
                executing.Clear();

                if (pauseTime.HasValue)
                    await Pause(pauseTime.Value);
            }

            public ValueTask Pause(double time = -1d)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                executor.Pause(tcs, time);
                return new ValueTask(tcs.Task);
            }
        }
    }
}
