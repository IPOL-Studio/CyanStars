using CyanStars.Framework.Logging;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Tap音符
    /// </summary>
    public class TapNote : BaseNote
    {
        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime, curViewTime);

            if (EvaluateHelper.IsMiss(Distance)) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.TapJudge(Data,Distance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime) //AutoMode下的更新
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (Distance <= 0)
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //销毁

                NoteJudger.TapJudge(Data, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return; //只处理按下的情况

            ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
            DestroySelf(false); //销毁

            NoteJudger.TapJudge(Data,Distance);
        }
    }
}
