using CyanStars.Framework;
using CyanStars.Framework.Logging;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
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
            dataModule.DistanceBarData.AddHeight(distance);

            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance); //获取评价类型

            LogJudgedInfo(new DefaultNoteJudgedInfo(data, et)); //Log

            dataModule.MaxScore += data.GetFullScore(); //更新理论最高分

            if (et != EvaluateType.Miss && et != EvaluateType.Bad) //Exact、Great、Right:加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    grade: et, currentDeviation: distance);
            }
            else //Bad、Miss:断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1,
                    EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, float.MaxValue);
            }


        }

        /// <summary>
        /// 处理hold音符头判
        /// </summary>
        public static EvaluateType HoldHeadJudge(NoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);

            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            if (et == EvaluateType.Bad || et == EvaluateType.Miss) //Bad、Miss:断combo，不计算杂率
            {
                //头判失败
                LogJudgedInfo(new HoldNoteHeadJudgedInfo(data, et));
                dataModule.MaxScore += data.GetFullScore();
                dataModule.RefreshPlayingData(-1, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, float.MaxValue);
            }
            else //Exact、Great、Right:不断combo，计算杂率
            {
                //头判成功
                LogJudgedInfo(new HoldNoteHeadJudgedInfo(data, et));
                dataModule.MaxScore++;
                dataModule.RefreshPlayingData(1, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, distance);
            }

            return et;
        }

        /// <summary>
        /// 处理hold音符miss
        /// </summary>
        public static void HoldMiss(NoteData data)
        {
            dataModule.DistanceBarData.AddHeightWithMiss();
            LogJudgedInfo(new HoldNoteJudgedInfo(data, EvaluateType.Miss, 0, 0));

            dataModule.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, 0, EvaluateType.Miss, float.MaxValue); // Miss：断combo，不计算杂率
        }

        /// <summary>
        /// 处理hold音符尾判
        /// </summary>
        public static void HoldTailJudge(NoteData data, float pressTimeLength, float value)
        {
            EvaluateType et = EvaluateHelper.GetHoldEvaluate(value); // 不判Bad

            LogJudgedInfo(new HoldNoteJudgedInfo(data, et, pressTimeLength, value));

            dataModule.MaxScore++;
            if (et != EvaluateType.Miss) // Exact、Great、Right：不加combo，不计算杂率
            {
                dataModule.RefreshPlayingData(0, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, float.MaxValue);
            }
            else // Miss：断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, float.MaxValue);
            }

        }

        /// <summary>
        /// 处理drag音符判定
        /// </summary>
        public static void DragJudge(NoteData data, bool isMiss)
        {
            EvaluateType et = EvaluateType.Exact;
            if (isMiss)
            {
                et = EvaluateType.Miss;
            }
            LogJudgedInfo(new DefaultNoteJudgedInfo(data, et));

            dataModule.MaxScore += data.GetFullScore();

            if (!isMiss) // Exact：加一combo、不计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(EvaluateType.Exact) * data.GetMagnification(),
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
        public static EvaluateType ClickHeadJudge(NoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);
            dataModule.MaxScore += 1;

            if (et != EvaluateType.Bad && et != EvaluateType.Miss) // Exact、Great、Right：加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(1, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, distance);

                LogJudgedInfo(new ClickNoteHeadJudgedInfo(data, et));
            }
            else // Bad、Miss：断combo，不计算杂率
            {
                //头判失败直接销毁

                LogJudgedInfo(new ClickNoteHeadJudgedInfo(data, et));
                dataModule.MaxScore += 1;
                dataModule.RefreshPlayingData(-1, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    et, float.MaxValue);
            }

            return et;
        }

        /// <summary>
        /// 处理click音符miss
        /// </summary>
        public static void ClickMiss(NoteData data)
        {
            dataModule.DistanceBarData.AddHeightWithMiss();
            LogJudgedInfo(new ClickNoteJudgedInfo(data, EvaluateType.Miss, 0));

            dataModule.MaxScore += 2;
            dataModule.RefreshPlayingData(-1, 0, EvaluateType.Miss, float.MaxValue); // Miss：断combo，不计算杂率
        }

        /// <summary>
        /// 处理click音符尾判
        /// </summary>
        public static void ClickTailJudge(NoteData data, float timeLength)
        {
            dataModule.MaxScore += 1;
            EvaluateType et = EvaluateHelper.GetClickEvaluate(timeLength);

            LogJudgedInfo(new ClickNoteJudgedInfo(data, et, timeLength));

            dataModule.RefreshPlayingData(0, EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                et, float.MaxValue);  // Exact、Out：不断combo，不计算杂率
        }

        /// <summary>
        /// 处理break音符判定
        /// </summary>
        public static void BreakJudge(NoteData data, float distance)
        {
            dataModule.DistanceBarData.AddHeight(distance);
            EvaluateType et = EvaluateHelper.GetTapEvaluate(distance);

            LogJudgedInfo(new DefaultNoteJudgedInfo(data, et));

            dataModule.MaxScore += data.GetFullScore();

            if (et != EvaluateType.Miss && et != EvaluateType.Bad) //Exact、Great、Right:加一combo，计算杂率
            {
                dataModule.RefreshPlayingData(addCombo: 1,
                    addScore: EvaluateHelper.GetScoreWithEvaluate(et) * data.GetMagnification(),
                    grade: et, currentDeviation: distance);
            }
            else //Bad、Miss:断combo，不计算杂率
            {
                dataModule.RefreshPlayingData(-1, 0, et, float.MaxValue);
            }
        }

        [HideInStackTrace]
        public static void LogJudgedInfo<T>(T info) where T : INoteJudgedInfo
        {
            dataModule.Logger.LogInfo(info.GetJudgeMessage());
        }
    }
}
