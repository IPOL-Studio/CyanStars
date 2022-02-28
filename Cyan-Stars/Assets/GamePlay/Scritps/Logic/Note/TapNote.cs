using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNote : BaseNote
{
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (logicTimer < EvaluateHelper.CheckInputEndTime)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Tap音符miss：{data}");
            GameManager.Instance.RefreshData(-1,-1,"Miss",float.MaxValue);
        }
    }

    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        if (inputType == InputType.Down)
        {
            DestroySelf(false);
            Debug.LogError($"Tap音符命中,评价:{EvaluateHelper.GetTapEvaluate(logicTimer)},{data}");
            GameManager.Instance.RefreshData(1,1,EvaluateHelper.GetTapEvaluate(logicTimer).ToString(),logicTimer - 0);
        }   
    }
}
