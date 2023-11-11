using System;
using MunNovel.Command;

namespace MunNovel
{
    public static class MunNovelRoot
    {
        public static bool IsInitialized { get; private set; }

        internal static SharedCommandCreators SharedCommandCreators { get; private set; }

        public static void Init()
        {
            if (IsInitialized)
                throw new InvalidOperationException("MunNovelRoot is already initialized");

            SharedCommandCreators = new SharedCommandCreators();
            IsInitialized = true;
        }

        // Unity maybe need it
        // Unity can call the method by asmref before update to CoreCLR and csproj
        internal static void Reset()
        {
            IsInitialized = false;
            SharedCommandCreators = null;
        }

        public static IExecutionContextBuilder CreateSimpleBuilder()
        {
            return new SimpleExecutionContextBuilder();
        }
    }
}