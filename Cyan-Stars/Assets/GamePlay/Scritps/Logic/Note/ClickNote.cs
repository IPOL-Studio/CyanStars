/*
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
*/
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
        if(LogicTimer < EvaluateHelper.CheckInputEndTime && !headSucess)
        {
            //没接住 miss
            DestroySelf();
            Debug.LogError($"Click音符miss：{data}");
            GameManager.Instance.maxScore += 2;
            GameManager.Instance.RefreshData(-1,-1,EvaluateType.Miss,float.MaxValue);
            return;
        }

        if (LogicTimer < EvaluateHelper.CheckInputEndTime)
        {
            float time = downTimePoint - LogicTimer;
            viewObject.CreateEffectObj();
            DestroySelf(false);
            Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
            GameManager.Instance.maxScore +=1;
            EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
            if(evaluateType == EvaluateType.Exact)
                GameManager.Instance.RefreshData(0,1,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
            else
                GameManager.Instance.RefreshData(0,0.5f,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
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
                    viewObject.CreateEffectObj();
                    EvaluateType et = EvaluateHelper.GetClickEvaluate(LogicTimer);
                    GameManager.Instance.maxScore += 1;
                    if(et != EvaluateType.Bad && et != EvaluateType.Miss)
                    {
                        GameManager.Instance.RefreshData(1,1,et,LogicTimer);
                        Debug.LogError($"Click音符头判命中：{data}");
                    }
                    else
                    {
                        //头判失败直接销毁
                        DestroySelf(false);
                        Debug.LogError($"Click头判失败,时间：{LogicTimer}，{data}");
                        GameManager.Instance.maxScore +=1;
                        GameManager.Instance.RefreshData(-1,-1,et,float.MaxValue);
                        return;
                    }
                }
                else
                {
                    downTimePoint = LogicTimer;
                }
                break;
            case InputType.Up:
                float time = downTimePoint - LogicTimer;
                viewObject.CreateEffectObj();
                DestroySelf(false);
                Debug.LogError($"Click音符命中，按住时间:{time}：{data}");
                GameManager.Instance.maxScore +=1;
                EvaluateType evaluateType = EvaluateHelper.GetClickEvaluate(time);
                if(evaluateType == EvaluateType.Exact)
                    GameManager.Instance.RefreshData(0,1,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
                else
                    GameManager.Instance.RefreshData(0,0.5f,EvaluateHelper.GetClickEvaluate(time),LogicTimer);
                break;
        }
    }
}