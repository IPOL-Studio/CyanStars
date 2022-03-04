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
    
    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);

        if (logicTimer < EvaluateHelper.CheckInputEndTime)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Click音符miss：{data}");
            GameManager.Instance.maxScore += 2;
            GameManager.Instance.RefreshData(-1,-1,"Miss",float.MaxValue);
        }
    }
    
    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        switch (inputType)
        {
            case InputType.Down:
                downTimePoint = logicTimer;
                break;

            
            case InputType.Up:
                float time = downTimePoint - logicTimer;
                DestroySelf(false);
                Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                GameManager.Instance.maxScore += 2;
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                if(evaluateType == EvaluateType.Exact)
                    GameManager.Instance.RefreshData(1,2,EvaluateHelper.GetClickEvaluate(time).ToString(),logicTimer);
                else
                    GameManager.Instance.RefreshData(1,1,EvaluateHelper.GetClickEvaluate(time).ToString(),logicTimer);
                break;
          
        }
        
    }
}
