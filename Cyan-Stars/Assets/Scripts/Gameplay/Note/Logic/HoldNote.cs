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

                if (!headChecked)
                {
                    //没进行过头判 被漏掉了 miss
                    NoteJudger.HoldMiss(Data);
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

                    NoteJudger.HoldTailJudge(Data,pressTimeLength,value);
                }


                DestroySelf();
            }
        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (EvaluateHelper.GetTapEvaluate(Distance) == EvaluateType.Exact && !headChecked)
            {
                headChecked = true;
                ViewObject?.CreateEffectObj(NoteData.NoteWidth);

                NoteJudger.HoldHeadJudge(Data, 0); // Auto Mode 杂率为0
            }

            if (Distance < holdCheckInputEndDistance)
            {
                ViewObject.DestroyEffectObj();
                DestroySelf();

                NoteJudger.HoldTailJudge(Data,holdLength,1);
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

                        EvaluateType et =  NoteJudger.HoldHeadJudge(Data, Distance);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            ViewObject.DestroyEffectObj();
                            DestroySelf(false);
                        }
                        else
                        {
                            //头判成功
                            firstPressTime = CurLogicTime;
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
