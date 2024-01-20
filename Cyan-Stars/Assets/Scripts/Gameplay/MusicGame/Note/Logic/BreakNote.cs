using System;
using CyanStars.Framework.Logging;

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
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

            if (EvaluateHelper.IsMiss(Distance)) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.BreakJudge(Data,Distance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (Distance <= 0)
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //销毁

                NoteJudger.BreakJudge(Data, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return;

            ViewObject.CreateEffectObj(0); //生成特效
            DestroySelf(false); //销毁

            NoteJudger.BreakJudge(Data,Distance);
        }
    }
}
