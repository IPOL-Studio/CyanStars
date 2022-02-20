using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tap音符，到达判定线时按下按键
/// </summary>
public class TapNote : BaseNote
{
    
    
    public TapNote(int trackIndex, NoteData data) : base(trackIndex, data)
    {

    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        
        if (timer <= EvaluateHelper.CheckInputEndTime)
        {
            //Miss了
            DestorySelf();
            Debug.LogError($"音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
    }

    public override void OnKeyDown()
    {
        base.OnKeyDown();

        if (timer > EvaluateHelper.CheckInputStartTime)
        {
            return;;
        }
        
        EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(timer);
        DestorySelf(false);
        
        Debug.LogError($"Tap音符命中，评价:{evaluateType}，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
    }


}
