using System;
using System.Collections.Generic;
using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Break音符
    /// </summary>
    public class BreakNote : BaseNote
    {
        public BreakNotePos Pos;

        public override void Init(BaseChartNoteData data, ChartContext context, ChartData chartData, NoteClip clip)
        {
            base.Init(data, context, chartData, clip);
            Pos = (data as BreakChartNoteData).BreakNotePos;
        }

        public override void OnUpdate(float curLogicTime, bool noEffect = false)
        {
            base.OnUpdate(curLogicTime, noEffect);

            if (EvaluateHelper.IsMiss(LogicTimeDistance))
            {
                // 在玩家游玩时达到 miss 时间点
                DestroySelf(); // 等待音符再过线一段距离后销毁
                NoteJudger.TapJudge(NoteData as TapChartNoteData, LogicTimeDistance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime, bool noEffect = false)
        {
            base.OnUpdateInAutoMode(curLogicTime, noEffect);

            if (LogicTimeDistance >= 0)
            {
                // 在自动播放时达到判定时间点
                if (!noEffect)
                    ViewObject.CreateEffectObj(0); // 生成特效，BreakNote 宽度设定为 0
                DestroySelf(false); // 让音符在判定线上立刻销毁
                NoteJudger.TapJudge(NoteData as TapChartNoteData, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return;

            ViewObject.CreateEffectObj(0); //生成特效
            DestroySelf(false); //销毁

            NoteJudger.BreakJudge(NoteData as BreakChartNoteData, LogicTimeDistance);
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
