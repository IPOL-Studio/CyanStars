using CyanStars.Framework.Helpers;

namespace CyanStars.Framework.Loggers
{
    public class NoteLogger : LoggerBase<NoteLogger>
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
