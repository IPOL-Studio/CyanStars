using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Click音符
    /// </summary>
    public class ClickNote : BaseNote, IMainTrackNotePos
    {
        private const float NoteWidth = 0.2f;

        /// <summary>
        /// 按下的时间点
        /// </summary>
        private float downTimePoint;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        public float Pos { get; set; }


        public override void Init(BaseChartNoteData data, ChartData chartData, NoteClip clip)
        {
            base.Init(data, chartData, clip);
            Pos = (data as ClickChartNoteData).Pos;
        }

        public override void OnUpdate(float curLogicTime)
        {
            base.OnUpdate(curLogicTime);

            if (EvaluateHelper.IsMiss(LogicTimeDistance))
            {
                if (!headChecked)
                {
                    //没接住 miss
                    DestroySelf();

                    NoteJudger.ClickMiss(NoteData as ClickChartNoteData);
                }
                else
                {
                    //头判成功过且超时未抬起 结算尾判
                    ViewObject.CreateEffectObj(NoteWidth);
                    DestroySelf(false);

                    float timeLength = curLogicTime - downTimePoint;
                    NoteJudger.ClickTailJudge(NoteData as ClickChartNoteData, timeLength);
                }
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance <= 0 && !headChecked)
            {
                headChecked = true;
                NoteJudger.ClickHeadJudge(NoteData as ClickChartNoteData, 0);
                NoteJudger.ClickTailJudge(NoteData as ClickChartNoteData, 0);

                ViewObject.CreateEffectObj(NoteWidth);
                DestroySelf(false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            switch (inputType)
            {
                case InputType.Down:
                    downTimePoint = CurLogicTime;

                    if (!headChecked)
                    {
                        headChecked = true;

                        //处理头判
                        EvaluateType et = NoteJudger.ClickHeadJudge(NoteData as ClickChartNoteData, LogicTimeDistance);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            ViewObject.CreateEffectObj(NoteWidth);
                            DestroySelf(false);
                        }
                    }

                    break;

                case InputType.Up:
                    if (!headChecked) return;

                    ViewObject.CreateEffectObj(NoteWidth);
                    DestroySelf(false);

                    float timeLength = CurLogicTime - downTimePoint;
                    NoteJudger.ClickTailJudge(NoteData as ClickChartNoteData, timeLength);
                    break;
            }
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
