using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EvaluateHelper
{
    /// <summary>
    /// 输入时间点大于这个时间 不处理输入
    /// </summary>
    public const float CheckInputStartTime = 0.2f;
    
    /// <summary>
    /// Tap Hold音符计时小于这个时间 就自动Miss
    /// </summary>
    public const float CheckInputEndTime = -0.231f;
    
    public static void ComputeDeviation(float hitTime,float holdLen = -1)
    {
        if(holdLen == -1)
        {
            GameManager.Instance.currentDeviation = hitTime;
            GameManager.Instance.deviationList.Add(hitTime);
        }
        else
        {
            hitTime -= holdLen;
            GameManager.Instance.currentDeviation = hitTime;
            GameManager.Instance.deviationList.Add(hitTime);
        }
    }
    /// <summary>
    /// 根据Tap音符命中时间获取评价类型
    /// </summary>
    public static EvaluateType GetTapEvaluate(float hitTime,float holdLen = -1)
    {
        //80
        if (hitTime <= 0.08f && hitTime >= -0.08f)
        {
            GameManager.Instance.excatNum ++;
            return EvaluateType.Exact;
        }   
        
        //81-140
        if (hitTime <= 0.14f && hitTime >= -0.14f)
        {
            GameManager.Instance.greatNum ++;
            return EvaluateType.Great;
        }

        //141-200（早）
        if (hitTime <= 0.2f)
        {
            GameManager.Instance.badNum ++;
            return EvaluateType.Bad;
        }

        //141-230（晚）
        if (hitTime >= -0.23f)
        {
            GameManager.Instance.rightNum ++;
            return EvaluateType.Right;
        }
        GameManager.Instance.missNum ++;
        return EvaluateType.Miss;
    }
    /// <summary>
    /// 根据Click音符命中时间获取评价类型
    /// </summary>
    public static EvaluateType GetClickEvaluate(float hitTime)
    {
        if (hitTime <= 0.12f)
        {
            GameManager.Instance.excatNum ++;
            return EvaluateType.Exact;
        }
        else if (hitTime <= 0.14f)
        {
            GameManager.Instance.greatNum ++;
            return EvaluateType.Great;
        }
        else if(hitTime <= 0.16f)
        {
            GameManager.Instance.rightNum ++;
            return EvaluateType.Right;
        }
        GameManager.Instance.badNum ++;
        return EvaluateType.Bad;
    }

    /// <summary>
    /// 根据Hold音符命中比例获取评价类型
    /// </summary>
    public static EvaluateType GetHoldEvaluate(float result)
    {
        if (result >= 0.95f)
        {
            GameManager.Instance.excatNum ++;
            return EvaluateType.Exact;
        }

        if (result >= 0.85f)
        {
            GameManager.Instance.greatNum ++;
            return EvaluateType.Great;
        }
        
        if (result >= 0.75f)
        {
            GameManager.Instance.rightNum ++;
            return EvaluateType.Right;
        }
        GameManager.Instance.missNum ++;
        return EvaluateType.Miss;
    }
}
