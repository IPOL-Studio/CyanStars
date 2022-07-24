

namespace CyanStars.Gameplay.MusicGame
{
    public struct DefaultNoteJudgeLogArgs : INoteJudgeLogArgs
    {
        private NoteData noteData;
        private EvaluateType evaluate;

        public DefaultNoteJudgeLogArgs(NoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeInfo()
        {
            return $"{noteData.Type}音符{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}";
        }
    }

    public struct ClickNoteJudgeLogArgs : INoteJudgeLogArgs
    {
        private NoteData noteData;
        private EvaluateType evaluate;
        private float holdTime;

        public ClickNoteJudgeLogArgs(NoteData data, EvaluateType evaluate, float holdTime)
        {
            this.noteData = data;
            this.evaluate = evaluate;
            this.holdTime = holdTime;
        }

        public string GetJudgeInfo()
        {
            return $"Click尾判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 按住时间{holdTime}";
        }
    }

    public struct ClickNoteHeadJudgeLogArgs : INoteJudgeLogArgs
    {
        private NoteData noteData;
        private EvaluateType evaluate;

        public ClickNoteHeadJudgeLogArgs(NoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeInfo()
        {
            return $"Click头判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}";
        }
    }

    public struct HoldNoteJudgeLogArgs : INoteJudgeLogArgs
    {
        private NoteData noteData;
        private EvaluateType evaluate;
        private float holdTime;
        private float holdRatio;

        public HoldNoteJudgeLogArgs(NoteData data, EvaluateType evaluate, float holdTime, float holdRatio)
        {
            this.noteData = data;
            this.evaluate = evaluate;
            this.holdTime = holdTime * 1000;
            this.holdRatio = holdRatio;
        }

        public string GetJudgeInfo()
        {
            return
                $"Hold尾判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 结束时间{noteData.HoldEndTime}, 按住时间{holdTime}, 按住比例{holdRatio}";
        }
    }

    public struct HoldNoteHeadJudgeLogArgs : INoteJudgeLogArgs
    {
        private NoteData noteData;
        private EvaluateType evaluate;

        public HoldNoteHeadJudgeLogArgs(NoteData data, EvaluateType evaluate)
        {
            this.noteData = data;
            this.evaluate = evaluate;
        }

        public string GetJudgeInfo()
        {
            return $"Hold头判{evaluate}, 位置{noteData.Pos}, 判定时间{noteData.JudgeTime}, 结束时间{noteData.HoldEndTime}";
        }
    }
}
