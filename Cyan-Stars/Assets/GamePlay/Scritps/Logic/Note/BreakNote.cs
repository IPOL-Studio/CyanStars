using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Break音符
/// </summary>
public class BreakNote : BaseNote
{
    public override bool IsInRange(float min, float max)
    {
        //Break音符的InRange判定有点特殊
        return Math.Abs(min - data.Pos) < 0.4f;
    }
    
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);
        if (LogicTimer < EvaluateHelper.CheckInputEndTime)
        {
            //没接住 miss
            DestroySelf();
            //Debug.LogError($"Break音符miss：{data}");
            LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, EvaluateType.Miss));
            GameManager.Instance.maxScore += 2;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
        }
    }

    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        if (inputType == InputType.Down)
        {
            viewObject.CreateEffectObj(0);
            DestroySelf(false);
            //Debug.LogError($"Break音符命中,{data}");
            EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(LogicTimer);
            LogHelper.NoteLogger.Log(new DefaultNoteJudgeLogArgs(data, evaluateType));
            GameManager.Instance.maxScore += 2;
            switch (evaluateType)
            {
                case EvaluateType.Exact:
                    GameManager.Instance.RefreshData(1,1*2,evaluateType,LogicTimer);
                    break;
                
                case  EvaluateType.Great:
                    GameManager.Instance.RefreshData(1,0.75f*2,evaluateType,LogicTimer);
                    break;
                
                case EvaluateType.Right:
                    GameManager.Instance.RefreshData(1,0.5f*2,evaluateType,LogicTimer);
                    break;

                case EvaluateType.Bad:
                    GameManager.Instance.RefreshData(-1,-1,evaluateType,LogicTimer);
                    break;

                case EvaluateType.Miss:
                    GameManager.Instance.RefreshData(-1,-1,evaluateType,float.MaxValue);
                    break;
            }
        }
    }
}
