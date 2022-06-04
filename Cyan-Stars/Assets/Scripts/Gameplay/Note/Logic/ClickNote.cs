using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;
using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Click音符
    /// </summary>
    public class ClickNote : BaseNote
    {
        /// <summary>
        /// 按下的时间点
        /// </summary>
        private float downTimePoint;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            base.OnUpdate(curLogicTime, curViewTime);

            if (Distance < EvaluateHelper.CheckInputEndDistance)
            {
                if (!headChecked)
                {
                    //没接住 miss
                    DestroySelf();

                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(Data, EvaluateType.Miss, 0));

                    DataModule.MaxScore += 2;
                    DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                }
                else
                {
                    //头判成功过超时未抬起 自动结算
                    float timeLength = curLogicTime - downTimePoint;
                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(timeLength);
                    DestroySelf(false);

                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(Data, evaluateType, timeLength));

                    DataModule.MaxScore += 1;
                    if (evaluateType == EvaluateType.Exact)
                        DataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                    else
                        DataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
                }

            }


        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact && !headChecked)
            {
                headChecked = true;
                DataModule.MaxScore += 2;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(Data, EvaluateType.Exact, 0));
                DataModule.RefreshPlayingData(1, 2, EvaluateType.Exact, 0);
                ViewObject.CreateEffectObj(NoteData.NoteWidth);
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

                        EvaluateType et = EvaluateHelper.GetTapEvaluate(Distance);
                        DataModule.MaxScore += 1;

                        if (et != EvaluateType.Bad && et != EvaluateType.Miss)
                        {
                            //头判成功
                            if (et == EvaluateType.Exact)
                                DataModule.RefreshPlayingData(1, 1, et, Distance);
                            else if (et == EvaluateType.Great)
                                DataModule.RefreshPlayingData(1, 0.75f, et, Distance);
                            else if (et == EvaluateType.Right)
                                DataModule.RefreshPlayingData(1, 0.5f, et, Distance);
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(Data, et));
                        }
                        else
                        {
                            //头判失败直接销毁
                            ViewObject.CreateEffectObj(NoteData.NoteWidth);
                            DestroySelf(false);
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(Data, et));
                            DataModule.MaxScore += 1;
                            DataModule.RefreshPlayingData(-1, -1, et,
                                et == EvaluateType.Miss ? float.MaxValue : CurLogicTime);
                        }
                    }

                    break;
                case InputType.Up:
                    if (!headChecked) return;

                    float timeLength = CurLogicTime - downTimePoint;
                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    DestroySelf(false);
                    DataModule.MaxScore += 1;
                    EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(timeLength);
                    LoggerManager.GetOrCreateLogger<NoteLogger>()
                        .Log(new ClickNoteJudgeLogArgs(Data, evaluateType, timeLength));
                    if (evaluateType == EvaluateType.Exact)
                        DataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                    else
                        DataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
                    break;
            }
        }


    }
}
