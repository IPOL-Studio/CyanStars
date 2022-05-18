using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Loggers;
using CyanStars.Gameplay.Evaluate;

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

        bool headSucess; //头部命中

        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);

            if (LogicTimer < EvaluateHelper.CheckInputEndTime && !headSucess)
            {
                //没接住 miss
                DestroySelf();
                //Debug.LogError($"Click音符miss：{data}");
                LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(data, EvaluateType.Miss, 0));
                dataModule.MaxScore += 2;
                dataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                return;
            }

            if (LogicTimer < EvaluateHelper.CheckInputEndTime)
            {
                float time = downTimePoint - LogicTimer;
                viewObject.CreateEffectObj(NoteData.NoteWidth);
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                DestroySelf(false);
                //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                dataModule.MaxScore += 1;
                LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(data, evaluateType, time));
                if (evaluateType == EvaluateType.Exact)
                    dataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                else
                    dataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
            }
        }

        public override void OnUpdateInAutoMode(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdateInAutoMode(deltaTime, noteSpeedRate);

            if (EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && !headSucess)
            {
                headSucess = true;
                dataModule.MaxScore += 2;
                LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(data, EvaluateType.Exact, 0));
                dataModule.RefreshPlayingData(1, 2, EvaluateType.Exact, 0);
                viewObject.CreateEffectObj(NoteData.NoteWidth);
                DestroySelf(false);
            }
        }

        public override void OnInput(InputType inputType)
        {
            base.OnInput(inputType);

            switch (inputType)
            {
                case InputType.Down:
                    if (!headSucess)
                    {
                        headSucess = true;
                        viewObject.CreateEffectObj(NoteData.NoteWidth);
                        EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                        dataModule.MaxScore += 1;
                        if (et != EvaluateType.Bad && et != EvaluateType.Miss)
                        {
                            if (et == EvaluateType.Exact)
                                dataModule.RefreshPlayingData(1, 1, et, LogicTimer);
                            else if (et == EvaluateType.Great)
                                dataModule.RefreshPlayingData(1, 0.75f, et, LogicTimer);
                            else if (et == EvaluateType.Right)
                                dataModule.RefreshPlayingData(1, 0.5f, et, LogicTimer);
                            //Debug.LogError($"Click音符头判命中：{data}");
                            LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(data, et));
                        }
                        else
                        {
                            //头判失败直接销毁
                            DestroySelf(false);
                            //Debug.LogError($"Click头判失败,时间：{LogicTimer}，{data}");
                            LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(data, et));
                            dataModule.MaxScore += 1;
                            dataModule.RefreshPlayingData(-1, -1, et,
                                et == EvaluateType.Miss ? float.MaxValue : LogicTimer);
                        }
                    }
                    else
                    {
                        downTimePoint = LogicTimer;
                    }

                    break;
                case InputType.Up:
                    if (!headSucess) return;
                    float time = downTimePoint - LogicTimer;
                    viewObject.CreateEffectObj(NoteData.NoteWidth);
                    DestroySelf(false);
                    //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                    dataModule.MaxScore += 1;
                    EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                    LoggerManager.GetOrAddLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(data, evaluateType, time));
                    if (evaluateType == EvaluateType.Exact)
                        dataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                    else
                        dataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
                    break;
            }
        }
    }
}