using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
using CyanStars.Gameplay.Evaluate;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// Hold音符
    /// </summary>
    public class HoldNote : BaseNote
    {
        /// <summary>
        /// Hold音符的检查输入结束时间
        /// </summary>
        private float holdCheckInputEndTime;

        /// <summary>
        /// Hold音符长度
        /// </summary>
        private float holdLength;

        /// <summary>
        /// 头判是否成功
        /// </summary>
        private bool headSuccess;

        /// <summary>
        /// 累计有效时长值(0-1)
        /// </summary>
        private float value;


        private int pressCount;
        private float pressTime;
        private float pressStartTime;

        public override void Init(NoteData data, NoteLayer layer)
        {
            base.Init(data, layer);

            holdLength = (data.HoldEndTime - data.JudgeTime) / 1000f;
            //hold结束时间点与长度相同
            holdCheckInputEndTime = -holdLength;
        }

        public override bool CanReceiveInput()
        {
            return LogicTimer <= EvaluateHelper.CheckInputStartTime && LogicTimer >= holdCheckInputEndTime;
        }

        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);

            if (pressCount > 0 && LogicTimer <= 0 || DataModule.IsAutoMode)
            {
                //只在音符区域内计算有效时间
                pressTime += deltaTime;
            }

            if (LogicTimer < holdCheckInputEndTime)
            {
                if (!headSuccess)
                {
                    //被漏掉了 miss
                    //Debug.LogError($"Hold音符miss：{data}");
                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(Data, EvaluateType.Miss, 0, 0));
                    DataModule.MaxScore += 2;
                    DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                }
                else
                {
                    ViewObject.DestroyEffectObj();
                    if (pressStartTime < 0) value = pressTime / (pressStartTime - LogicTimer);
                    else value = pressTime / holdLength;

                    EvaluateType et = EvaluateHelper.GetHoldEvaluate(value);
                    //Debug.LogError($"Hold音符命中，百分比:{value},评价:{et},{data}");
                    LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteJudgeLogArgs(Data, et, pressTime, value));
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

                DestroySelf();
            }
        }

        public override void OnUpdateInAutoMode(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdateInAutoMode(deltaTime, noteSpeedRate);

            if (EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && !headSuccess)
            {
                headSuccess = true;
                DataModule.MaxScore++;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, EvaluateType.Exact));
                DataModule.RefreshPlayingData(1, 1, EvaluateType.Exact, 0);
                ViewObject.CreateEffectObj(NoteData.NoteWidth);
            }

            if (LogicTimer < holdCheckInputEndTime)
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

                    if (!headSuccess)
                    {
                        //判断头判评价
                        EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                        if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                        {
                            //头判失败直接销毁
                            DestroySelf(false);
                            //Debug.LogError($"Hold头判失败,时间：{LogicTimer}，{data}");
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, et));
                            DataModule.MaxScore += 2;
                            DataModule.RefreshPlayingData(-1, -1, et, float.MaxValue);
                            return;
                        }

                        //Debug.LogError($"Hold头判成功,时间：{LogicTimer}，{data}");
                        LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new HoldNoteHeadJudgeLogArgs(Data, et));
                        DataModule.MaxScore++;
                        if (et == EvaluateType.Exact)
                            DataModule.RefreshPlayingData(1, 1, et, LogicTimer);
                        else if (et == EvaluateType.Great)
                            DataModule.RefreshPlayingData(1, 0.75f, et, LogicTimer);
                        else if (et == EvaluateType.Right)
                            DataModule.RefreshPlayingData(1, 0.5f, et, LogicTimer);
                        pressStartTime = LogicTimer;
                    }

                    //头判成功
                    headSuccess = true;
                    if (pressCount == 0) ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    pressCount++;
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
