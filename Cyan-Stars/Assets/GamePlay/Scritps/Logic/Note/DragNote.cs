using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drag音符
/// </summary>
public class DragNote : BaseNote
{
    private bool isHit;
    
    public override bool CanReceiveInput()
    {
        return LogicTimer <= EvaluateHelper.DragTimeRange && LogicTimer >= -EvaluateHelper.DragTimeRange;
    }

    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (isHit && LogicTimer <= 0 )
        {
            DestroySelf(false);
            return;
        }
        
        if (LogicTimer < -EvaluateHelper.DragTimeRange)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Drag音符miss：{data}");
            GameManager.Instance.maxScore ++;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
        }
    }

    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        switch (inputType)
        {

            case InputType.Press:

                if (isHit)
                {
                    return;
                }
                viewObject.CreateEffectObj();
                Debug.LogError($"Drag音符命中：{data}");
                GameManager.Instance.maxScore ++;
                GameManager.Instance.RefreshData(1,1,EvaluateType.Exact,LogicTimer);
                if (LogicTimer > 0)
                {
                    //早按准点放
                    isHit = true;
                }
                else
                {
                    //晚按即刻放
                    DestroySelf(false);
                }
                break;
        }
    }
}
