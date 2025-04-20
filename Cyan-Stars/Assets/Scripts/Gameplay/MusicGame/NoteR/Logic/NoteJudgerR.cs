using CyanStars.Framework;
using CyanStars.Framework.Logging;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符判定器
    /// </summary>
    public static class NoteJudgerR
    {
        private static MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();

        /// <summary>
        /// 获取每个音符的总分
        /// </summary>
        private static float GetFullScore(NoteType noteType)
        {
            return noteType switch
            {
                NoteType.Tap => 1,
                NoteType.Hold => 2, // 包括头判和尾判的总分
                NoteType.Break => 2,
                NoteType.Drag => 0.25f,
                NoteType.Click => 2, // 包括头判和尾判的总分
                _ => throw new System.NotFiniteNumberException()
            };
        }

        /// <summary>
        /// 获取每一次判定的分数倍率
        /// </summary>
        public static float GetMagnification(NoteType noteType)
        {
            return noteType switch
            {
                NoteType.Tap => 1,
                NoteType.Hold => 1, // 判定两次
                NoteType.Break => 2,
                NoteType.Drag => 0.25f,
                NoteType.Click => 1, // 判定两次
                _ => throw new System.NotFiniteNumberException()
            };
        }


        /// <summary>
        /// 处理tap音符判定
        /// </summary>
        public static void TapJudge(TapChartNoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);

            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance); //获取评价类型

            LogJudgedInfoR(new TapNoteJudgedInfoR(data, et)); //Log

            dataModule.MusicGamePlayData.MaxScore += GetFullScore(NoteType.Tap); //更新理论最高分

            if (et != EvaluateType.Miss && et != EvaluateType.Bad) //Exact、Great、Right:加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Tap),
                    grade: et, currentDeviation: distance);
            }
            else //Bad、Miss:断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Tap),
                    et, float.MaxValue);
            }
        }

        /// <summary>
        /// 处理hold音符头判
        /// </summary>
        public static EvaluateType HoldHeadJudge(HoldChartNoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);

            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            if (et == EvaluateType.Bad || et == EvaluateType.Miss) //Bad、Miss:断combo，不计算杂率
            {
                //头判失败
                LogJudgedInfoR(new HoldNoteHeadJudgedInfoR(data, et));
                dataModule.MusicGamePlayData.MaxScore += GetFullScore(NoteType.Hold);
                dataModule.RefreshPlayingData(-1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Hold),
                    et, float.MaxValue);
            }
            else //Exact、Great、Right:不断combo，计算杂率
            {
                //头判成功
                LogJudgedInfoR(new HoldNoteHeadJudgedInfoR(data, et));
                dataModule.MusicGamePlayData.MaxScore++;
                dataModule.RefreshPlayingData(1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Hold),
                    et, distance);
            }

            return et;
        }

        /// <summary>
        /// 处理hold音符miss
        /// </summary>
        public static void HoldMiss(HoldChartNoteData data)
        {
            dataModule.DistanceBarData.AddHeightWithMiss();
            LogJudgedInfoR(new HoldNoteJudgedInfoR(data, EvaluateType.Miss, 0, 0));

            dataModule.MusicGamePlayData.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, 0, EvaluateType.Miss, float.MaxValue); // Miss：断combo，不计算杂率
        }

        /// <summary>
        /// 处理hold音符尾判
        /// </summary>
        public static void HoldTailJudge(HoldChartNoteData data, float pressTimeLength, float value)
        {
            EvaluateType et = EvaluateHelper.GetHoldEvaluate(value); // 不判Bad

            LogJudgedInfoR(new HoldNoteJudgedInfoR(data, et, pressTimeLength, value));

            dataModule.MusicGamePlayData.MaxScore++;
            if (et != EvaluateType.Miss) // Exact、Great、Right：不加combo，不计算杂率
            {
                dataModule.RefreshPlayingData(0,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Hold),
                    et, float.MaxValue);
            }
            else // Miss：断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Hold),
                    et, float.MaxValue);
            }
        }

        /// <summary>
        /// 处理drag音符判定
        /// </summary>
        public static void DragJudge(DragChartNoteData data, bool isMiss)
        {
            EvaluateType et = EvaluateType.Exact;
            if (isMiss)
            {
                et = EvaluateType.Miss;
            }

            LogJudgedInfoR(new DragNoteJudgedInfoR(data, et));

            dataModule.MusicGamePlayData.MaxScore += GetFullScore(NoteType.Drag);

            if (!isMiss) // Exact：加一combo、不计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(EvaluateType.Exact) * GetMagnification(NoteType.Drag),
                    grade: EvaluateType.Exact, currentDeviation: float.MaxValue);
            }
            else // Miss：断combo、不计算杂率
            {
                dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue); //更新数据
            }
        }

        /// <summary>
        /// 处理click音符头判
        /// </summary>
        public static EvaluateType ClickHeadJudge(ClickChartNoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            dataModule.MusicGamePlayData.MaxScore += 1;

            if (et != EvaluateType.Bad && et != EvaluateType.Miss) // Exact、Great、Right：加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Click),
                    et, distance);

                LogJudgedInfoR(new ClickNoteHeadJudgedInfoR(data, et));
            }
            else // Bad、Miss：断combo，不计算杂率
            {
                //头判失败直接销毁

                LogJudgedInfoR(new ClickNoteHeadJudgedInfoR(data, et));
                dataModule.MusicGamePlayData.MaxScore += 1;
                dataModule.RefreshPlayingData(-1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Click),
                    et, float.MaxValue);
            }

            return et;
        }

        /// <summary>
        /// 处理click音符miss
        /// </summary>
        public static void ClickMiss(ClickChartNoteData data)
        {
            dataModule.DistanceBarData.AddHeightWithMiss();
            LogJudgedInfoR(new ClickNoteJudgedInfoR(data, EvaluateType.Miss, 0));

            dataModule.MusicGamePlayData.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, 0, EvaluateType.Miss, float.MaxValue); // Miss：断combo，不计算杂率
        }

        /// <summary>
        /// 处理click音符尾判
        /// </summary>
        public static void ClickTailJudge(ClickChartNoteData data, float timeLength)
        {
            dataModule.MusicGamePlayData.MaxScore += 1;
            EvaluateType et = EvaluateHelper.GetClickEvaluate(timeLength);

            LogJudgedInfoR(new ClickNoteJudgedInfoR(data, et, timeLength));

            dataModule.RefreshPlayingData(0, EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Click),
                et, float.MaxValue); // Exact、Out：不断combo，不计算杂率
        }

        /// <summary>
        /// 处理break音符判定
        /// </summary>
        public static void BreakJudge(BreakChartNoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);

            LogJudgedInfoR(new BreakNoteJudgedInfoR(data, et));

            dataModule.MusicGamePlayData.MaxScore += GetFullScore(NoteType.Break);

            if (et != EvaluateType.Miss && et != EvaluateType.Bad) //Exact、Great、Right:加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * GetMagnification(NoteType.Break),
                    grade: et, currentDeviation: distance);
            }
            else //Bad、Miss:断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1, 0, et, float.MaxValue);
            }
        }

        [HideInStackTrace]
        public static void LogJudgedInfoR<T>(T info) where T : INoteJudgedInfoR
        {
            dataModule.Logger.LogInfo(info.GetJudgeMessage());
        }
    }
}
