using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Drag音符
    /// </summary>
    public class DragNote : BaseNote
    {
        private bool isHit;

        public override bool CanReceiveInput()
        {
            return Distance <= EvaluateHelper.DragJudgeDistanceRange && Distance >= -EvaluateHelper.DragJudgeDistanceRange;
        }

        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime,curViewTime);

            if (isHit && Distance <= 0) //接住并过线
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //立即销毁
                return;
            }

            if (Distance < -EvaluateHelper.DragJudgeDistanceRange) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, EvaluateType.Miss)); //Log

                DataModule.MaxScore += Data.GetFullScore(); //更新最理论高分
                DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue); //更新数据
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime,curViewTime);

            if (CanReceiveInput() && !isHit)
            {
                isHit = true;

                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //立即销毁

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

            if (inputType != InputType.Press) return; //只处理按下的情况

            if (isHit) return; //已经接住了


            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, EvaluateType.Exact)); //Log

            DataModule.MaxScore += Data.GetFullScore(); //更新理论最高分

            DataModule.RefreshPlayingData(addCombo: 1,
                addScore: EvaluateHelper.GetScoreWithEvaluate(EvaluateType.Exact) * Data.GetMagnification(),
                grade: EvaluateType.Exact, currentDeviation: float.MaxValue); //更新数据


            if (Distance > 0)
            {
                //早按准点放
                isHit = true;
            }
            else
            {
                //晚按即刻放
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false);
            }
        }
    }
}
