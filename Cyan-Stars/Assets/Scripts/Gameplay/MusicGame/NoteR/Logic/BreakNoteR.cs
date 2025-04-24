using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Break音符
    /// </summary>
    public class BreakNoteR : BaseNoteR
    {
        public override void OnUpdate(float curLogicTime)
        {
            base.OnUpdate(curLogicTime);

            if (EvaluateHelper.IsMiss(LogicTimeDistance)) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudgerR.BreakJudge(NoteData as BreakChartNoteData, LogicTimeDistance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance <= 0)
            {
                ViewObject.CreateEffectObj(0); //生成特效，BreakNote宽度设定为0
                DestroySelf(false); //销毁

                NoteJudgerR.BreakJudge(NoteData as BreakChartNoteData, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return;

            ViewObject.CreateEffectObj(0); //生成特效
            DestroySelf(false); //销毁

            NoteJudgerR.BreakJudge(NoteData as BreakChartNoteData, LogicTimeDistance);
        }
    }
}
