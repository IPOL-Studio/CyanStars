using CyanStars.Framework.Logger;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.Loggers
{
    public class NoteLogger : LoggerBase
    {
        public void Log<T>(T args) where T : struct, INoteJudgeLogArgs
        {
            LogHelper.Log(args.GetJudgeInfo(), LogLevel);
        }
    }

    public interface INoteJudgeLogArgs
    {
        public string GetJudgeInfo();
    }
}
