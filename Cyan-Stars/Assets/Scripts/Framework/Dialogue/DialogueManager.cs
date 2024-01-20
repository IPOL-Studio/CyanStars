using CyanStars.Framework.Logging;
using MunNovel;

namespace CyanStars.Framework.Dialogue
{
    public class DialogueManager : BaseManager
    {
        public override int Priority { get; }


        public override void OnInit()
        {
            MunNovelRoot.Init();
        }

        public override void OnUpdate(float deltaTime)
        {

        }

        public IExecutionContextBuilder CreateSimpleBuilder(string loggerCategoryName = null)
        {
            var builder = MunNovelRoot.CreateSimpleBuilder();
            if (!string.IsNullOrEmpty(loggerCategoryName) && !string.IsNullOrWhiteSpace(loggerCategoryName))
            {
                builder.ConfigureLogger(() =>
                    new MunNovelLoggerProxy(
                        loggerCategoryName,
                        GameRoot.Logger.GetOrCreateLogger,
                        categoryName => GameRoot.Logger.RemoveLogger(categoryName)
                    )
                );
            }

            return builder;
        }
    }
}
