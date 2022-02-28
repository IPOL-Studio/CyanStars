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
        return logicTimer <= EvaluateHelper.DragTimeRange && logicTimer >= -EvaluateHelper.DragTimeRange;
    }

    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (isHit && logicTimer <= 0 )
        {
            DestroySelf(false);
            return;
        }
        
        if (logicTimer < -EvaluateHelper.DragTimeRange)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Drag音符miss：{data}");
            GameManager.Instance.RefreshData(-1,-1,"Miss",float.MaxValue);
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
                
                Debug.LogError($"Drag音符命中：{data}");
                GameManager.Instance.RefreshData(1,1,EvaluateType.Exact.ToString(),logicTimer);
                if (logicTimer > 0)
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
