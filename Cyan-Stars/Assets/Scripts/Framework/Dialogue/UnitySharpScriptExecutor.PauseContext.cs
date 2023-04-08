using System;
using System.Threading;
using System.Threading.Tasks;

namespace CyanStars.Framework.Dialogue
{
    public partial class UnitySharpScriptExecutor
    {
        private struct PauseContext
        {
            public TaskCompletionSource<object> TaskSource;
            public CancellationTokenSource CancellationSource;

            public bool IsNull => TaskSource == null;

            public PauseContext(TaskCompletionSource<object> taskSource, CancellationTokenSource cancellationSource = null)
            {
                this.TaskSource = taskSource;
                this.CancellationSource = cancellationSource;
            }

        }
    }
}
