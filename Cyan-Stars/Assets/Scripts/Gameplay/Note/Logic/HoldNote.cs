using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;
using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Hold音符
    /// </summary>
    public class HoldNote : BaseNote
    {
        /// <summary>
        /// Hold音符的检查输入结束距离
        /// </summary>
        private float holdCheckInputEndDistance;

        /// <summary>
        /// Hold音符长度
        /// </summary>
        private float holdLength;

        /// <summary>
        /// 是否进行过头判
        /// </summary>
        private bool headChecked;

        /// <summary>
        /// 累计有效时长比例(0-1)
        /// </summary>
        private float value;

        /// <summary>
        /// 按住的按键数量
        /// </summary>
        private int pressCount;

        /// <summary>
        /// 累计有效时长值
        /// </summary>
        private float pressTimeLength;

        /// <summary>
        /// 首次按下时间
        /// </summary>
        private float firstPressTime;

        public override void Init(NoteData data, NoteLayer layer)
        {
            base.Init(data, layer);

            holdLength = (data.HoldEndTime - data.JudgeTime) / 1000f;
            holdCheckInputEndDistance = -holdLength;//hold结束时间点与长度相同
        }

        public override bool CanReceiveInput()
        {
            return Distance <= EvaluateHelper.CheckInputStartDistance && Distance >= holdCheckInputEndDistance;
        }

        public override void OnUpdate(float curLogicTime,float curViewTime)
        {
            float deltaTime = curLogicTime - CurLogicTime;

            base.OnUpdate(curLogicTime, curViewTime);

            if (pressCount > 0 && Distance <= 0 && Distance >= holdCheckInputEndDistance)
            {
                //只在hold音符区域内有按住时，累计有效时长
                pressTimeLength += deltaTime;
            }

            if (Distance < holdCheckInputEndDistance)
            {
                //整条hold都跑完了
                DestroySelf();

                if (!headChecked)
                {
                    //没进行过头判 被漏掉了 miss
                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(Data, EvaluateType.Miss, 0, 0));

                    DataModule.MaxScore += 2;
                    DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                }
                else
                {
                    //进行过头判 计算按住比例
                    ViewObject.DestroyEffectObj();
                    if (firstPressTime > JudgeTime)
                    {
                        //首次按下时间在判定时间之后的情况下 以首次按下时间为起点计算总长度
                        value = pressTimeLength / (Data.HoldEndTime/1000f - firstPressTime);
                    }
                    else
                    {
                        //否则以hold长度作为总长度
                        value = pressTimeLength / holdLength;
                    }

                    EvaluateType et = EvaluateHelper.GetHoldEvaluate(value);

                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(Data, et, pressTimeLength, value));

                    DataModule.MaxScore++;
                    if (et == EvaluateType.Exact)
                        DataModule.RefreshPlayingData(0, 1, et, float.MaxValue);
                    else if (et == EvaluateType.Great)
                        DataModule.RefreshPlayingData(0, 0.75f, et, float.MaxValue);
                    else if (et == EvaluateType.Right)
                        DataModule.RefreshPlayingData(0, 0.5f, et, float.MaxValue);
                    else
                        DataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
                }


            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact && !headChecked)
            {
                headChecked = true;
                DataModule.MaxScore++;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, EvaluateType.Exact));
                DataModule.RefreshPlayingData(1, 1, EvaluateType.Exact, 0);
                ViewObject.CreateEffectObj(NoteData.NoteWidth);
            }

            if (Distance < holdCheckInputEndDistance)
            {
                ViewObject.DestroyEffectObj();
                DataModule.MaxScore++;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(Data, EvaluateType.Exact, holdLength, 1));
                DataModule.RefreshPlayingData(0, 1, EvaluateType.Exact, float.MaxValue);
                DestroySelf();
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            switch (inputType)
            {
                case InputType.Down:

                    if (pressCount == 0)
                    {
                        ViewObject.CreateEffectObj(NoteData.NoteWidth);
                        pressCount++;
                    }


                    if (!headChecked)
                    {
                        headChecked = true;
                        //头判处理

                        EvaluateType et = EvaluateHelper.GetTapEvaluate(Distance);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            DestroySelf(false);
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, et));
                            DataModule.MaxScore += 2;
                            DataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
                        }
                        else
                        {
                            //头判成功
                            firstPressTime = CurLogicTime;

                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, et));
                            DataModule.MaxScore++;
                            if (et == EvaluateType.Exact)
                                DataModule.RefreshPlayingData(1, 1, et, Distance);
                            else if (et == EvaluateType.Great)
                                DataModule.RefreshPlayingData(1, 0.75f, et, Distance);
                            else if (et == EvaluateType.Right)
                                DataModule.RefreshPlayingData(1, 0.5f, et, Distance);
                        }


                    }



                    break;

                case InputType.Up:

                    if (pressCount > 0)
                    {
                        pressCount--;
                        if (pressCount == 0) ViewObject.DestroyEffectObj();
                    }

                    break;
            }
        }
    }
}
