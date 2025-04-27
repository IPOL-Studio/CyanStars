using CyanStars.Gameplay.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// Click音符
    /// </summary>
    public class ClickNoteR : BaseNoteR, INotePos
    {
        public float Pos { get; set; }

        /// <summary>
        /// 按下的时间点
        /// </summary>
        private float downTimePoint;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        private const float NoteWidth = 0.2f;


        public override void Init(BaseChartNoteData data, ChartData chartData)
        {
            base.Init(data, chartData);
            Pos = (data as DragChartNoteData).Pos;
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

                    NoteJudgerR.ClickMiss(NoteData as ClickChartNoteData);
                }
                else
                {
                    //头判成功过且超时未抬起 结算尾判
                    ViewObject.CreateEffectObj(NoteWidth);
                    DestroySelf(false);

                    float timeLength = curLogicTime - downTimePoint;
                    NoteJudgerR.ClickTailJudge(NoteData as ClickChartNoteData, timeLength);
                }
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance <= 0 && !headChecked)
            {
                headChecked = true;
                NoteJudgerR.ClickHeadJudge(NoteData as ClickChartNoteData, 0);
                NoteJudgerR.ClickTailJudge(NoteData as ClickChartNoteData, 0);

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
                        EvaluateType et = NoteJudgerR.ClickHeadJudge(NoteData as ClickChartNoteData, LogicTimeDistance);
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
                    NoteJudgerR.ClickTailJudge(NoteData as ClickChartNoteData, timeLength);
                    break;
            }
        }
    }
}
