using CyanStars.Framework.Helpers;

namespace CyanStars.Framework.Loggers
{
    public class NoteLogger : LoggerBase<NoteLogger>
    {
        public LogLevelType LogLevel { get; set; } = LogLevelType.Log;

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
