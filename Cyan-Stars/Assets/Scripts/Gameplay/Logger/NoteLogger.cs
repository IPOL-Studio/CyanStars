using CyanStars.Framework.Utils;
using CyanStars.Framework.Logger;

namespace CyanStars.Gameplay.Logger
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
