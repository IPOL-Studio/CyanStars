using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Tap音符
    /// </summary>
    public class TapNote : BaseNote
    {
        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime, curViewTime);

            if (Distance < EvaluateHelper.CheckInputEndDistance) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.TapJudge(Data,Distance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime) //AutoMode下的更新
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact)
            {
                ViewObject.CreateEffectObj(NoteData.NoteWidth); //生成特效
                DestroySelf(false); //销毁

                NoteJudger.TapJudge(Data,Distance);
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
