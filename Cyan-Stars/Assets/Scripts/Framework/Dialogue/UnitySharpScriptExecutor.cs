using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MunNovel.Command;
using MunNovel.Executor;
using MunNovel.SharpScript;
using CyanStars.Framework.Timer;

namespace CyanStars.Framework.Dialogue
{
    public delegate void ScriptExecutorStateChanged(ScriptExecuteState oldState, ScriptExecuteState newState);

    public partial class UnitySharpScriptExecutor : IScriptExecutor
    {
        public event ScriptExecutorStateChanged OnStateChanged;

        private ScriptExecuteState state;
        public ScriptExecuteState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    ScriptExecuteState oldState = state;
                    state = value;
                    OnStateChanged?.Invoke(oldState, value);
                }
            }
        }

        private IScript script;
        private IScriptContext context;
        private IScriptExecutorOperationHandler operationHandler;

        private PauseContext pauseContext;

        private readonly List<Task> ExecutingTasks = new List<Task>(10);
        public readonly List<ICommand> ExecutedCommands = new List<ICommand>(10);

        private CancellationTokenSource cancelCommandCts;
        private CancellationToken CommandCancelToken
        {
            get
            {
                if (this.cancelCommandCts?.IsCancellationRequested ?? true)
                {
                    this.cancelCommandCts = new CancellationTokenSource();
                }
                return this.cancelCommandCts.Token;
            }
        }


        public UnitySharpScriptExecutor(IScriptExecutorOperationHandler operationHandler)
        {
            context = new ScriptContext(new CommandBuffer(this));
            this.operationHandler = operationHandler;
        }

        public UnitySharpScriptExecutor(IScriptContext context, IScriptExecutorOperationHandler operationHandler)
        {
            this.context = context;
            this.operationHandler = operationHandler;
        }

        public async Task Load(IScript script, CancellationToken cancellationToken = default)
        {
            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            this.script = script;
            State = ScriptExecuteState.Loading;

            await script.PreLoad(cancellationToken);
            State = ScriptExecuteState.Loaded;
        }

        public void Play()
        {
            if (State == ScriptExecuteState.NoScript)
            {
                throw new ScriptExecuteException("Not load Script");
            }

            if (State == ScriptExecuteState.Loading)
            {
                throw new ScriptExecuteException("Script loading");
            }

            if (State != ScriptExecuteState.Loaded)
            {
                return;
            }

            State = ScriptExecuteState.Playing;
            Execute();
        }

        public bool TryCompleteCurrentExecuting()
        {
            if (this.ExecutingTasks.Count > 0 && !this.cancelCommandCts.IsCancellationRequested)
            {
                this.cancelCommandCts.Cancel();
                return true;
            }
            return false;
        }

        private async void Execute()
        {
            await script.Execute(context);
            State = ScriptExecuteState.Done;
        }

        private void Pause(TaskCompletionSource<object> tcs, double time)
        {
            if (State != ScriptExecuteState.Playing)
            {
                UnityEngine.Debug.LogError("script is not playing state, can't pause");
                tcs.SetCanceled();
                return;
            }

            this.pauseContext = new PauseContext(tcs);
            State = ScriptExecuteState.Pause;

            // time <= 0 时，将暂停script执行的控制权移交给外部实现
            if (time <= 0)
            {
                ThrowPause();
            }
            else
            {
                Pause(tcs.Task, time);
            }
        }

        private async void ThrowPause()
        {
            await operationHandler.OnPause();
            Resume();
        }

        private void Pause(Task pauseTask, double time)
        {
            void WaitPauseCallback(object userdata)
            {
                var task = (Task)userdata;
                if (!task.IsCompleted)
                {
                    Resume();
                }
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            this.pauseContext.CancellationSource = cts;
            GameRoot.Timer.GetTimer<IntervalTimer>().Add((float)time, WaitPauseCallback, pauseTask, 1, cts.Token);
        }

        public void Resume()
        {
            if (this.pauseContext.IsNull)
                return;

            TaskCompletionSource<object> tcs = this.pauseContext.TaskSource;
            this.pauseContext = default;
            State = ScriptExecuteState.Playing;
            tcs.SetResult(null);
        }

        private void CommandsExecuted(IList<ICommand> commands)
        {
            if ((commands?.Count ?? 0) > 0)
            {
                this.ExecutedCommands.AddRange(commands);
                commands.Clear();
            }
        }
    }
}
