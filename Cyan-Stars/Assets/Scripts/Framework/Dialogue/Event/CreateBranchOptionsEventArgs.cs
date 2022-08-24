using System;
using System.Collections.Generic;
using CyanStars.Framework.Pool;

namespace CyanStars.Framework.Dialogue
{
    public class CreateBranchOptionsEventArgs : EventArgs, IReference
    {
        public const string EventName = nameof(CreateBranchOptionsEventArgs);

        public IReadOnlyList<BranchOption> Options { get; private set; }

        public static CreateBranchOptionsEventArgs Create(IReadOnlyList<BranchOption> options)
        {
            CreateBranchOptionsEventArgs eventArgs = ReferencePool.Get<CreateBranchOptionsEventArgs>();
            eventArgs.Options = options;
            return eventArgs;
        }

        public void Clear()
        {
            Options = default;
        }
    }
}
