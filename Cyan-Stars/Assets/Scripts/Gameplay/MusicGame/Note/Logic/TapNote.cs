using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class TapNote : BaseNote, IMainTrackNotePos
    {
        private const float NoteWidth = 0.2f;
        public float Pos { get; set; }

        public override void Init(BaseChartNoteData data, ChartContext context, ChartData chartData, NoteClip clip)
        {
            base.Init(data, context, chartData, clip);
            Pos = (data as TapChartNoteData).Pos;
        }

        public override void OnUpdate(float curLogicTime)
        {
            base.OnUpdate(curLogicTime);

            if (EvaluateHelper.IsMiss(LogicTimeDistance)) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.TapJudge(NoteData as TapChartNoteData, LogicTimeDistance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance >= 0)
            {
                ViewObject.CreateEffectObj(NoteWidth); //生成特效
                DestroySelf(false); //销毁

                NoteJudger.TapJudge(NoteData as TapChartNoteData, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return; //只处理按下的情况

            ViewObject.CreateEffectObj(NoteWidth); //生成特效
            DestroySelf(false); //销毁

            NoteJudger.TapJudge(NoteData as TapChartNoteData, LogicTimeDistance);
        }

        /// <summary>
        /// 是否在指定输入范围内
        /// </summary>
        public override bool IsInInputRange(float min, float max)
        {
            float left = Pos;
            float right = Pos + NoteWidth;

            //3种情况可能重合 1.最左侧在范围内 2.最右侧在范围内 3.中间部分在范围内
            bool result = (left >= min && left <= max)
                          || (right >= min && right <= max)
                          || (left <= min && right >= max);

            return result;
        }
    }
}
