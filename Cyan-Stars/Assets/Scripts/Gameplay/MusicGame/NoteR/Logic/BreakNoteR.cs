using System;
using CyanStars.Gameplay.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Break音符
    /// </summary>
    public class BreakNoteR : BaseNoteR
    {
        public BreakNotePos Pos;

        public override void Init(BaseChartNoteData data, ChartData chartData)
        {
            base.Init(data, chartData);
            Pos = (data as BreakChartNoteData).BreakNotePos;
        }

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

        public override bool IsInInputRange(float min, float max)
        {
            // TODO: 把 BreakNote 的输入处理得更优雅一点，顺便加上陀螺仪输入检测
            float p = Pos switch
            {
                BreakNotePos.Left => -1,
                BreakNotePos.Right => 2,
                _ => throw new ArgumentOutOfRangeException()
            };

            return Mathf.Abs(p - min) <= float.Epsilon;
        }
    }
}
