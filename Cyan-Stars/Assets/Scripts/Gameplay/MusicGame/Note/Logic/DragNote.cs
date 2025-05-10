using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public class DragNote : BaseNote, IMainTrackNotePos
    {
        public float Pos { get; set; }

        private bool isHit;

        private const float NoteWidth = 0.2f;


        public override bool CanReceiveInput()
        {
            return LogicTimeDistance <= EvaluateHelper.DragJudgeDistanceRange &&
                   LogicTimeDistance >= -EvaluateHelper.DragJudgeDistanceRange;
        }

        public override void Init(BaseChartNoteData data, ChartData chartData, NoteClip clip)
        {
            base.Init(data, chartData, clip);
            Pos = (data as DragChartNoteData).Pos;
        }

        public override void OnUpdate(float curLogicTime)
        {
            base.OnUpdate(curLogicTime);

            if (isHit && LogicTimeDistance >= 0) //接住并过线
            {
                ViewObject.CreateEffectObj(NoteWidth); //生成特效
                DestroySelf(false); //立即销毁
                return;
            }

            if (LogicTimeDistance > EvaluateHelper.DragJudgeDistanceRange) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudger.DragJudge(NoteData as DragChartNoteData, true);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance >= 0)
            {
                isHit = true;

                ViewObject.CreateEffectObj(NoteWidth); //生成特效
                DestroySelf(false); //立即销毁

                NoteJudger.DragJudge(NoteData as DragChartNoteData, false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Press) return; //只处理按下的情况
            if (isHit) return; //已经接住了

            if (LogicTimeDistance < 0)
            {
                //早按准点放
                isHit = true;
            }
            else
            {
                //晚按即刻放
                ViewObject.CreateEffectObj(NoteWidth); //生成特效
                DestroySelf(false);
            }

            NoteJudger.DragJudge(NoteData as DragChartNoteData, false);
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
