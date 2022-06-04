using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    public class TapNote : BaseNote
    {
        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime, curViewTime);

            if (Distance < EvaluateHelper.CheckInputEndDistance) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, EvaluateType.Miss)); //Log

                DataModule.MaxScore += Data.GetFullScore(); //更新最理论高分
                DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue); //更新数据
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime) //AutoMode下的更新
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact)
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //销毁

                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, EvaluateType.Exact)); //Log

                DataModule.MaxScore += Data.GetFullScore(); //更新理论最高分
                DataModule.RefreshPlayingData(addCombo: 1,
                    addScore: Data.GetFullScore(),
                    grade: EvaluateType.Exact, currentDeviation: float.MaxValue); //更新数据
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return; //只处理按下的情况

            ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
            DestroySelf(false); //销毁

            EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(Distance); //获取评价类型

            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, evaluateType)); //Log

            DataModule.MaxScore += Data.GetFullScore(); //更新理论最高分
            DataModule.RefreshPlayingData(addCombo: 1,
                addScore: EvaluateHelper.GetScoreWithEvaluate(evaluateType) * Data.GetMagnification(),
                grade: evaluateType, currentDeviation: CurLogicTime); //更新数据
        }
    }
}
