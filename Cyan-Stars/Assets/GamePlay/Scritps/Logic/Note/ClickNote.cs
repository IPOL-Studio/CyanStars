using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Click音符
/// </summary>
public class ClickNote : BaseNote
{
    /// <summary>
    /// 按下的时间点
    /// </summary>
    private float downTimePoint;
    bool headSucess;//头部命中
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if(EvaluateHelper.GetTapEvaluate(LogicTimer) == EvaluateType.Exact && GameManager.Instance.isAutoMode && !headSucess)
        {
            headSucess = true;
            GameManager.Instance.maxScore+=2;
            GameManager.Instance.RefreshData(1, 2, EvaluateType.Exact, 0);
            viewObject.CreateEffectObj(data.Width);
            DestroySelf(false);
        }

        if(LogicTimer < EvaluateHelper.CheckInputEndTime && !headSucess)
        {
            //没接住 miss
            DestroySelf();
            //Debug.LogError($"Click音符miss：{data}");
            LogHelper.NoteLogger.Log(new ClickNoteJudgeLogArgs(data, EvaluateType.Miss));
            GameManager.Instance.maxScore += 2;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
            return;
        }

        if (LogicTimer < EvaluateHelper.CheckInputEndTime)
        {
            float time = LogicTimer - downTimePoint;
            viewObject.CreateEffectObj(data.Width);
            EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
            DestroySelf(false);
            //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
            GameManager.Instance.maxScore += 1;
            LogHelper.NoteLogger.Log(new ClickNoteJudgeLogArgs(data, evaluateType, time));
            if(evaluateType == EvaluateType.Exact)
                GameManager.Instance.RefreshData(0,1,evaluateType,LogicTimer);
            else
                GameManager.Instance.RefreshData(0,0.5f,evaluateType,LogicTimer);
            return;
        }
    }
    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        switch (inputType)
        {
            case InputType.Down:
                if(!headSucess)
                {
                    headSucess = true;
                    viewObject.CreateEffectObj(data.Width);
                    EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                    GameManager.Instance.maxScore += 1;
                    if(et != EvaluateType.Bad && et != EvaluateType.Miss)
                    {
                        if(et == EvaluateType.Exact)
                            GameManager.Instance.RefreshData(1,1,et,LogicTimer);
                        else if(et == EvaluateType.Great)
                            GameManager.Instance.RefreshData(1,0.75f,et,LogicTimer);
                        else if(et == EvaluateType.Right)
                            GameManager.Instance.RefreshData(1,0.5f,et,LogicTimer);
                        //Debug.LogError($"Click音符头判命中：{data}");
                        LogHelper.NoteLogger.Log(new ClickNoteHeadJudgeLogArgs(data, et, LogicTimer));
                    }
                    else
                    {
                        //头判失败直接销毁
                        DestroySelf(false);
                        //Debug.LogError($"Click头判失败,时间：{LogicTimer}，{data}");
                        LogHelper.NoteLogger.Log(new ClickNoteHeadJudgeLogArgs(data, et, LogicTimer));
                        GameManager.Instance.maxScore +=1;
                        if(et == EvaluateType.Miss)GameManager.Instance.RefreshData(-1,-1,et,float.MaxValue);
                        else if(et == EvaluateType.Bad)GameManager.Instance.RefreshData(-1,-1,et,LogicTimer);
                        return;
                    }
                }
                else
                {
                    downTimePoint = LogicTimer;
                }
                break;
            case InputType.Up:
                float time = LogicTimer - downTimePoint;
                viewObject.CreateEffectObj(data.Width);
                DestroySelf(false);
                //Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                GameManager.Instance.maxScore +=1;
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                LogHelper.NoteLogger.Log(new ClickNoteJudgeLogArgs(data, evaluateType, time));
                if(evaluateType == EvaluateType.Exact)
                    GameManager.Instance.RefreshData(0,1,evaluateType,LogicTimer);
                else
                    GameManager.Instance.RefreshData(0,0.5f,evaluateType,LogicTimer);
                break;
        }
    }
}