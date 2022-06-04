using System;
using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;
using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    /// <summary>s
    /// Break音符
    /// </summary>
    public class BreakNote : BaseNote
    {
        public override bool IsInInputRange(float min, float max)
        {
            //Break音符的InRange判定有点特殊
            //左-1
            //右2
            return Mathf.Abs(Pos - min) <= float.Epsilon;
        }

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

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact)
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //销毁

                DataModule.MaxScore += Data.GetFullScore(); //更新理论最高分
                DataModule.RefreshPlayingData(addCombo: 1,
                    addScore: Data.GetFullScore(),
                    grade: EvaluateType.Exact, currentDeviation: float.MaxValue); //更新数据
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return;

            ViewObject.CreateEffectObj(0); //生成特效
            DestroySelf(false); //销毁

            EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(Distance); //获取评价类型

            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(Data, evaluateType)); //Log

            DataModule.MaxScore += Data.GetFullScore(); //更新理论最高分
            DataModule.RefreshPlayingData(addCombo: 1,
                addScore: EvaluateHelper.GetScoreWithEvaluate(evaluateType) * Data.GetMagnification(),
                grade: evaluateType, currentDeviation: Distance); //更新数据
        }
    }
}
