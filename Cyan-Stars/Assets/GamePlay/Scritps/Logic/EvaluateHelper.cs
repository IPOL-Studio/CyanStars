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
    
    
    /// <summary>
    /// 根据Tap音符命中时间获取评价类型
    /// </summary>
    public static EvaluateType GetTapEvaluate(float hitTime)
    {
        //80
        if (hitTime <= 0.08f && hitTime >= -0.08f)
        {
            return EvaluateType.Exact;
        }   
        
        //81-140
        if (hitTime <= 0.14f && hitTime >= -0.14f)
        {
            return EvaluateType.Great;
        }

        //141-200（早）
        if (hitTime <= 0.2f)
        {
            return EvaluateType.Bad;
        }

        //141-230（晚）
        if (hitTime >= -0.23f)
        {
            return EvaluateType.Right;
        }

        return EvaluateType.Miss;
    }

    /// <summary>
    /// 根据Hold音符命中比例获取评价类型
    /// </summary>
    public static EvaluateType GetHoldEvaluate(float result)
    {
        if (result >= 0.95f)
        {
            return EvaluateType.Exact;
        }

        if (result >= 0.85f)
        {
            return EvaluateType.Great;
        }
        
        if (result >= 0.75f)
        {
            return EvaluateType.Right;
        }

        return EvaluateType.Miss;
    }
}
