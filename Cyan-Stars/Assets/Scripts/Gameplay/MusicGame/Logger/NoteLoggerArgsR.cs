using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    public interface INoteJudgedInfoR
    {
        public string GetJudgeMessage();
    }

    public struct TapNoteJudgedInfoR : INoteJudgedInfoR
    {
        private TapChartNoteData noteData;
        private EvaluateType evaluate;

        public TapNoteJudgedInfoR(TapChartNoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeMessage()
        {
            return $"{noteData.Type}音符{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}";
        }
    }

    public struct HoldNoteHeadJudgedInfoR : INoteJudgedInfoR
    {
        private HoldChartNoteData noteData;
        private EvaluateType evaluate;

        public HoldNoteHeadJudgedInfoR(HoldChartNoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeMessage()
        {
            return
                $"Hold头判{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}, 结束拍{noteData.EndJudgeBeat.ToFloat()}";
        }
    }

    public struct HoldNoteJudgedInfoR : INoteJudgedInfoR
    {
        private HoldChartNoteData noteData;
        private EvaluateType evaluate;
        private float holdTime;
        private float holdRatio;

        public HoldNoteJudgedInfoR(HoldChartNoteData data, EvaluateType evaluate, float holdTime, float holdRatio)
        {
            this.noteData = data;
            this.evaluate = evaluate;
            this.holdTime = holdTime * 1000;
            this.holdRatio = holdRatio;
        }

        public string GetJudgeMessage()
        {
            return
                $"Hold尾判{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}, 结束拍{noteData.EndJudgeBeat.ToFloat()}, 按住时间{holdTime}, 按住比例{holdRatio}";
        }
    }

    public struct DragNoteJudgedInfoR : INoteJudgedInfoR
    {
        private DragChartNoteData noteData;
        private EvaluateType evaluate;

        public DragNoteJudgedInfoR(DragChartNoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeMessage()
        {
            return $"{noteData.Type}音符{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}";
        }
    }

    public struct ClickNoteHeadJudgedInfoR : INoteJudgedInfoR
    {
        private ClickChartNoteData noteData;
        private EvaluateType evaluate;

        public ClickNoteHeadJudgedInfoR(ClickChartNoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeMessage()
        {
            return $"Click头判{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}";
        }
    }

    public struct ClickNoteJudgedInfoR : INoteJudgedInfoR
    {
        private ClickChartNoteData noteData;
        private EvaluateType evaluate;
        private float holdTime;

        public ClickNoteJudgedInfoR(ClickChartNoteData data, EvaluateType evaluate, float holdTime)
        {
            this.noteData = data;
            this.evaluate = evaluate;
            this.holdTime = holdTime;
        }

        public string GetJudgeMessage()
        {
            return $"Click尾判{evaluate}, 位置{noteData.Pos}, 判定拍{noteData.JudgeBeat.ToFloat()}, 按住时间{holdTime}";
        }
    }

    public struct BreakNoteJudgedInfoR : INoteJudgedInfoR
    {
        private BreakChartNoteData noteData;
        private EvaluateType evaluate;

        public BreakNoteJudgedInfoR(BreakChartNoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeMessage()
        {
            return $"{noteData.Type}音符{evaluate}, 位置{noteData.BreakNotePos}, 判定拍{noteData.JudgeBeat.ToFloat()}";
        }
    }
}
