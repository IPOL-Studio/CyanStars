using CyanStars.Framework.Logger;

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
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
        /// 头判命中时间
        /// </summary>
        private float headCheckTime;

        /// <summary>
        /// 是否有按住的输入
        /// </summary>
        private bool isPressed;

        /// <summary>
        /// 累计有效时长值
        /// </summary>
        private float pressTimeLength;

        /// <summary>
        /// 累计有效时长比例(0-1)
        /// </summary>
        private float value;



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

            if (!isPressed)
            {
                //这里isPressed为false 就表示从上一次OnUpdate到这次OnUpdate之间没有Press类型的输入
                ViewObject?.DestroyEffectObj();
            }
            else
            {
                //重置Press标记
                isPressed = false;

                if (Distance <= 0 && Distance >= holdCheckInputEndDistance)
                {
                    //只在hold音符区域内有按住时，累计有效时长
                    pressTimeLength += deltaTime;
                }
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
                    //总长度默认为hold长度
                    float allLength = holdLength;
                    if (headCheckTime > JudgeTime)
                    {
                        //头判晚命中的情况下 以头判命中时间为起点计算总长度
                        allLength = Data.HoldEndTime / 1000f - headCheckTime;
                    }

                    //修正因累加deltaTime可能导致的按住时长比总时长大的情况
                    pressTimeLength = Mathf.Clamp(pressTimeLength, pressTimeLength, allLength);

                    //计算按住比例
                    value = pressTimeLength / allLength;

                    NoteJudger.HoldTailJudge(Data,pressTimeLength,value);

                    ViewObject?.DestroyEffectObj();
                }
                DestroySelf();
            }

        }

        public override void OnUpdateInAutoMode(float curLogicTime,float curViewTime)
        {
            base.OnUpdateInAutoMode(curLogicTime, curViewTime);

            if (!headChecked && Distance <= 0)
            {
                headChecked = true;

                (ViewObject as HoldViewObject)?.OpenFlicker();

                ViewObject?.CreateEffectObj(NoteData.NoteWidth);

                NoteJudger.HoldHeadJudge(Data, 0); // Auto Mode 杂率为0
            }

            if (Distance < holdCheckInputEndDistance)
            {
                ViewObject?.DestroyEffectObj();
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

                    if (headChecked)
                    {
                        return;
                    }

                    //头判处理
                    EvaluateType et =  NoteJudger.HoldHeadJudge(Data, Distance);
                    if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                    {
                        //头判失败直接销毁
                        DestroySelf(false);
                    }
                    else
                    {
                        //头判成功
                        headChecked = true;
                        headCheckTime = CurLogicTime;
                        isPressed = true;
                        ViewObject.CreateEffectObj(NoteData.NoteWidth);
                        //头判处理
                        (ViewObject as HoldViewObject)?.OpenFlicker();
                    }

                    break;

                case InputType.Press:

                    if (!headChecked)
                    {
                       return;
                    }

                    //头判命中后 有Press输入 才开始计算命中时长
                    isPressed = true;
                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    break;
            }
        }
    }
}
