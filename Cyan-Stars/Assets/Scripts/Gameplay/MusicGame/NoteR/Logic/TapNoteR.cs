namespace CyanStars.Gameplay.MusicGame
{
    public class TapNoteR : BaseNoteR
    {
        public float Pos;

        private const float NoteWidth = 0.2f;

        public override void Init(BaseChartNoteData data, ChartData chartData)
        {
            base.Init(data, chartData);
            Pos = (data as TapChartNoteData).Pos;
        }

        public override void OnUpdate(float curLogicTime)
        {
            base.OnUpdate(curLogicTime);

            if (EvaluateHelper.IsMiss(LogicTimeDistance)) //没接住Miss
            {
                DestroySelf(); //延迟销毁

                NoteJudgerR.TapJudge(NoteData as TapChartNoteData, LogicTimeDistance);
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime)
        {
            base.OnUpdateInAutoMode(curLogicTime);

            if (LogicTimeDistance >= 0)
            {
                ViewObject.CreateEffectObj(NoteWidth); //生成特效
                DestroySelf(false); //销毁

                NoteJudgerR.TapJudge(NoteData as TapChartNoteData, 0); // Auto Mode 杂率为0
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            if (inputType != InputType.Down) return; //只处理按下的情况

            ViewObject.CreateEffectObj(NoteWidth); //生成特效
            DestroySelf(false); //销毁

            NoteJudgerR.TapJudge(NoteData as TapChartNoteData, LogicTimeDistance);
        }
    }
}
