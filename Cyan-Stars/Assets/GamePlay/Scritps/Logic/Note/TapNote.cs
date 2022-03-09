using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : BaseNote
{
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (LogicTimer < EvaluateHelper.CheckInputEndTime)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Tap音符miss：{data}");
            GameManager.Instance.maxScore ++;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
        }
    }

    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        if (inputType == InputType.Down)
        {
            viewObject.CreateEffectObj();
            DestroySelf(false);
            EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(LogicTimer);
            Debug.LogError($"Tap音符命中,评价:{evaluateType},{data}");
            GameManager.Instance.maxScore ++;
            if(evaluateType != EvaluateType.Miss && evaluateType != EvaluateType.Bad)
                GameManager.Instance.RefreshData(1,1,evaluateType,LogicTimer - 0);
            else
                GameManager.Instance.RefreshData(-1,-1,evaluateType,float.MaxValue);
        }   
    }
}
