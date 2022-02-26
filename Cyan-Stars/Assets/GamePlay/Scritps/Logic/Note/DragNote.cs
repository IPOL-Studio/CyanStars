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
        return timer <= EvaluateHelper.DragTimeRange && timer >= -EvaluateHelper.DragTimeRange;
    }

    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (isHit && timer <= 0 )
        {
            DestorySelf(false);
            return;
        }
        
        if (timer < -EvaluateHelper.DragTimeRange)
        {
            //没接住 miss
            DestorySelf();
            Debug.LogError($"Drag音符miss：{data}");
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
                if (timer > 0)
                {
                    //早按准点放
                    isHit = true;
                }
                else
                {
                    //晚按即刻放
                    DestorySelf(false);
                }
                break;
        }
    }
}
