using System;
using UnityEngine;

/// <summary>
/// Hold音符
/// </summary>
public class HoldNote : BaseNote
{

    /// <summary>
    /// 头判是否成功
    /// </summary>
    private bool headSucess;
    
    /// <summary>
    /// 累计有效时长值(0-1)
    /// </summary>
    private float value;

    /// <summary>
    /// 按下的时间点
    /// </summary>
    private float downTimePoint;
    
    
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (timer < (EvaluateHelper.CheckInputEndTime - data.HoldEndTime))
        {
            if (!headSucess)
            {
                //被漏掉了 miss
                Debug.LogError($"Hold音符miss：{data}");
            }
            else
            {
                if (downTimePoint != 0)
                {
                    //按下后一直持续到结尾的话 也要算一下分
                    float time = downTimePoint - timer;
                    value += time / data.HoldEndTime;
                }
                
                
                EvaluateType et =  EvaluateHelper.GetHoldEvaluate(value);
                Debug.LogError($"Hold音符命中，百分比:{value},评价:{et},{data}");
            }
            
            DestorySelf();
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
                    //判断头判评价
                    EvaluateType et = EvaluateHelper.GetTapEvaluate(timer);
                    if (et == EvaluateType.Bad)
                    {
                        //头判失败直接销毁
                        DestorySelf(false);
                        Debug.LogError($"Hold音符miss：{data}");
                        return;
                    }
                    else
                    {
                        Debug.LogError($"Hold头判成功,时间：{timer}，{data}");
                    }
                }
                
                //头判成功 记录按下时间
                headSucess = true;
                downTimePoint = timer;
                
                break;

            
            case InputType.Up:

                if (downTimePoint != 0)
                {
                    //此次有效时长
                    float time = downTimePoint - timer;
                    value += time / data.HoldEndTime;
                    
                    //重置按下时间点
                    downTimePoint = 0;
                }
                
                break;
          
        }
        
    }
}