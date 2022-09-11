using CyanStars.Framework.Utils;
using CyanStars.Framework.Logger;

namespace CyanStars.Gameplay.MusicGame
{
    public sealed class NoteLogger : ILogger
    {
        public event LogCallback OnLog;

        public void Log<T>(T args) where T : struct, INoteJudgeLogArgs
        {
            string message = args.GetJudgeInfo();

            OnLog?.Invoke(message, LogLevelType.Log);
            LogHelper.Log(message, LogLevelType.Log);
        }
    }

    public interface INoteJudgeLogArgs
    {
        public string GetJudgeInfo();
    }
}
