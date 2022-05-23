using CyanStars.Framework.Logger;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Logger;
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

        bool headSuccess; //头部命中

        public override void OnUpdate(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdate(deltaTime, noteSpeedRate);

            if (LogicTimer < EvaluateHelper.CheckInputEndTime && !headSuccess)
            {
                //没接住 miss
                DestroySelf();
                //Debug.LogError($"Click音符miss：{data}");
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(Data, EvaluateType.Miss, 0));
                DataModule.MaxScore += 2;
                DataModule.RefreshPlayingData(-1, -1, EvaluateType.Miss, float.MaxValue);
                return;
            }

            if (LogicTimer < EvaluateHelper.CheckInputEndTime)
            {
                float time = downTimePoint - LogicTimer;
                ViewObject.CreateEffectObj(NoteData.NoteWidth);
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                DestroySelf(false);
                //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                DataModule.MaxScore += 1;
                LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteJudgeLogArgs(Data, evaluateType, time));
                if (evaluateType == EvaluateType.Exact)
                    DataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                else
                    DataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
            }
        }

        public override void OnUpdateInAutoMode(float deltaTime, float noteSpeedRate)
        {
            base.OnUpdateInAutoMode(deltaTime, noteSpeedRate);

            if (EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && !headSuccess)
            {
                headSuccess = true;
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
                    if (!headSuccess)
                    {
                        headSuccess = true;
                        ViewObject.CreateEffectObj(NoteData.NoteWidth);
                        EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                        DataModule.MaxScore += 1;
                        if (et != EvaluateType.Bad && et != EvaluateType.Miss)
                        {
                            if (et == EvaluateType.Exact)
                                DataModule.RefreshPlayingData(1, 1, et, LogicTimer);
                            else if (et == EvaluateType.Great)
                                DataModule.RefreshPlayingData(1, 0.75f, et, LogicTimer);
                            else if (et == EvaluateType.Right)
                                DataModule.RefreshPlayingData(1, 0.5f, et, LogicTimer);
                            //Debug.LogError($"Click音符头判命中：{data}");
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(Data, et));
                        }
                        else
                        {
                            //头判失败直接销毁
                            DestroySelf(false);
                            //Debug.LogError($"Click头判失败,时间：{LogicTimer}，{data}");
                            LoggerManager.GetOrCreateLogger<NoteLogger>().Log(new ClickNoteHeadJudgeLogArgs(Data, et));
                            DataModule.MaxScore += 1;
                            DataModule.RefreshPlayingData(-1, -1, et,
                                et == EvaluateType.Miss ? float.MaxValue : LogicTimer);
                        }
                    }
                    else
                    {
                        downTimePoint = LogicTimer;
                    }

                    break;
                case InputType.Up:
                    if (!headSuccess) return;
                    float time = downTimePoint - LogicTimer;
                    ViewObject.CreateEffectObj(NoteData.NoteWidth);
                    DestroySelf(false);
                    //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                    DataModule.MaxScore += 1;
                    EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                    LoggerManager.GetOrCreateLogger<NoteLogger>()
                        .Log(new ClickNoteJudgeLogArgs(Data, evaluateType, time));
                    if (evaluateType == EvaluateType.Exact)
                        DataModule.RefreshPlayingData(0, 1, evaluateType, float.MaxValue);
                    else
                        DataModule.RefreshPlayingData(0, 0.5f, evaluateType, float.MaxValue);
                    break;
            }
        }
    }
}
