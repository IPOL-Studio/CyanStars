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

        if (LogicTimer < EvaluateHelper.CheckInputEndTime)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Click音符miss：{data}");
            GameManager.Instance.maxScore += 2;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
        }
    }
    
    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        switch (inputType)
        {
            case InputType.Down:
                downTimePoint = LogicTimer;
                break;

            
            case InputType.Up:
                float time = downTimePoint - LogicTimer;
                viewObject.CreateEffectObj();
                DestroySelf(false);
                Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                GameManager.Instance.maxScore += 2;
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                if(evaluateType == EvaluateType.Exact)
                    GameManager.Instance.RefreshData(1,2,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
                else
                    GameManager.Instance.RefreshData(1,1,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
                break;
          
        }
        
    }
}
