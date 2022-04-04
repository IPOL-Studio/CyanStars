using CyanStars.Framework.Helpers;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Loggers;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note.Logic
{
    public class TapNote : BaseNote
    {
        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);
            if (EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && GameManager.Instance.isAutoMode)
            {
                viewObject.CreateEffectObj(data.Width);
                DestroySelf(false);
                GameManager.Instance.maxScore++;
                GameManager.Instance.RefreshData(1, 1, EvaluateType.Exact, 0);
                return;
            }

            if (LogicTimer < EvaluateHelper.CheckInputEndTime)
            {
                //没接住 miss
                DestroySelf();
                //Debug.LogError($"Tap音符miss：{data}");
                LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, EvaluateType.Miss));
                GameManager.Instance.maxScore++;
                GameManager.Instance.RefreshData(-1, -1, EvaluateType.Miss, float.MaxValue);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType == InputType.Down)
            {
                viewObject.CreateEffectObj(data.Width);
                DestroySelf(false);
                EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(LogicTimer);
                //Debug.LogError($"Tap音符命中,评价:{evaluateType},{data}");s
                LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, evaluateType));
                GameManager.Instance.maxScore++;
                switch (evaluateType)
                {
                    case EvaluateType.Exact:
                        GameManager.Instance.RefreshData(1, 1, evaluateType, LogicTimer);
                        break;

                    case EvaluateType.Great:
                        GameManager.Instance.RefreshData(1, 0.75f, evaluateType, LogicTimer);
                        break;

                    case EvaluateType.Right:
                        GameManager.Instance.RefreshData(1, 0.5f, evaluateType, LogicTimer);
                        break;

                    case EvaluateType.Bad:
                        GameManager.Instance.RefreshData(-1, -1, evaluateType, LogicTimer);
                        break;

                    case EvaluateType.Miss:
                        GameManager.Instance.RefreshData(-1, -1, evaluateType, float.MaxValue);
                        break;
                }
            }
        }
    }
}
