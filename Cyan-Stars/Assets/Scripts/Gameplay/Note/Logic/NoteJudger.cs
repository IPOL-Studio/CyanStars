using CyanStars.Framework;
using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Evaluate;
using CyanStars.Gameplay.Logger;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 音符判定器
    /// </summary>
    public static class NoteJudger
    {
        private static MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();

         /// <summary>
        /// 处理tap音符判定
        /// </summary>
        public static void TapJudge(NoteData data, float distance)
        {

            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance); //获取评价类型

            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(data, et)); //Log

            dataModule.MaxScore += data.GetFullScore(); //更新理论最高分

            if (et != EvaluateType.Miss)
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    grade: et, currentDeviation: distance);
            }
            else
            {
                dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
            }


        }

        /// <summary>
        /// 处理hold音符头判
        /// </summary>
        public static EvaluateType HoldHeadJudge(NoteData data, float distance)
        {
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            if (et == EvaluateType.Bad || et == EvaluateType.Miss)
            {
                //头判失败
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(data, et));
                dataModule.MaxScore += 2;
                dataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
            }
            else
            {
                //头判成功
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(data, et));
                dataModule.MaxScore++;
                if (et == EvaluateType.Exact)
                {
                    dataModule.RefreshPlayingData(1, 1, et, distance);
                }

                else if (et == EvaluateType.Great)
                {
                    dataModule.RefreshPlayingData(1, 0.75f, et, distance);
                }

                else if (et == EvaluateType.Right)
                {
                    dataModule.RefreshPlayingData(1, 0.5f, et, distance);
                }
            }

            return et;
        }

        /// <summary>
        /// 处理hold音符miss
        /// </summary>
        public static void HoldMiss(NoteData data)
        {
            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(data, EvaluateType.Miss, 0, 0));

            dataModule.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
        }

        /// <summary>
        /// 处理hold音符尾判
        /// </summary>
        public static void HoldTailJudge(NoteData data,float pressTimeLength, float value)
        {
            EvaluateType et = EvaluateHelper.GetHoldEvaluate(value);

            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(data, et, pressTimeLength, value));

            dataModule.MaxScore++;
            if (et == EvaluateType.Exact)
            {
                dataModule.RefreshPlayingData(0, 1, et, float.MaxValue);
            }
            else if (et == EvaluateType.Great)
            {
                dataModule.RefreshPlayingData(0, 0.75f, et, float.MaxValue);
            }

            else if (et == EvaluateType.Right)
            {
                dataModule.RefreshPlayingData(0, 0.5f, et, float.MaxValue);
            }
            else
            {
                dataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
            }

        }

        /// <summary>
        /// 处理drag音符判定
        /// </summary>
        public static void DragJudge(NoteData data,bool isMiss)
        {
            EvaluateType et = EvaluateType.Exact;
            if (isMiss)
            {
                et = EvaluateType.Miss;
            }
            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(data,et));

            dataModule.MaxScore += data.GetFullScore();

            if (!isMiss)
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(EvaluateType.Exact) * data.GetMagnification(),
                    grade: EvaluateType.Exact, currentDeviation: float.MaxValue);
            }
            else
            {
                dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue); //更新数据
            }
        }

        /// <summary>
        /// 处理click音符头判
        /// </summary>
        public static EvaluateType ClickHeadJudge(NoteData data, float distance)
        {
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            dataModule.MaxScore += 1;

            if (et != EvaluateType.Bad && et != EvaluateType.Miss)
            {
                //头判成功
                if (et == EvaluateType.Exact)
                {
                    dataModule.RefreshPlayingData(1, 1, et, distance);
                }

                else if (et == EvaluateType.Great)
                {
                    dataModule.RefreshPlayingData(1, 0.75f, et, distance);
                }

                else if (et == EvaluateType.Right)
                {
                    dataModule.RefreshPlayingData(1, 0.5f, et, distance);
                }
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(data, et));
            }
            else
            {
                //头判失败直接销毁

                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(data, et));
                dataModule.MaxScore += 1;
                dataModule.RefreshPlayingData(-1, -1, et,
                    et == EvaluateType.Miss ? float.MaxValue : distance);
            }

            return et;
        }

        /// <summary>
        /// 处理click音符miss
        /// </summary>
        public static void ClickMiss(NoteData data)
        {
            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(data, EvaluateType.Miss, 0));

            dataModule.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
        }

        /// <summary>
        /// 处理click音符尾判
        /// </summary>
        public static void ClickTailJudge(NoteData data,float timeLength)
        {
            dataModule.MaxScore += 1;
            EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(timeLength);

            LoggerManager.GetOrCreateLogger<NoteLogger>()
                .Log(new ClickNoteJudgeLogArgs(data, evaluateType, timeLength));

            if (evaluateType == EvaluateType.Exact)
            {
                dataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
            }

            else
            {
                dataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
            }
        }

        /// <summary>
        /// 处理break音符判定
        /// </summary>
        public static void BreakJudge(NoteData data, float distance)
        {
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);

            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new DefaultNoteJudgeLogArgs(data, et));

            dataModule.MaxScore += data.GetFullScore();

            if (et != EvaluateType.Miss)
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    grade: et, currentDeviation: distance);
            }
            else
            {
                dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
            }
        }
    }
}
