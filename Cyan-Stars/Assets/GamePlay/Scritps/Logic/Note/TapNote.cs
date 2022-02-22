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
            Debug.Log($"音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
            RefleshPlayingUI(EvaluateType.Miss,false,-1,-1);
        }
    }

    public override void OnKeyDown()
    {
        base.OnKeyDown();
        //Debug.Log($"音符被按下，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");

        if (timer > EvaluateHelper.CheckInputStartTime)
        {
            return;
        }
        
        EvaluateType evaluateType = EvaluateHelper.GetTapEvaluate(timer);
        if(evaluateType == EvaluateType.Exact || evaluateType == EvaluateType.Great)
            RefleshPlayingUI(evaluateType,true,1,1);
        else if(evaluateType == EvaluateType.Right || evaluateType == EvaluateType.Bad)
            RefleshPlayingUI(evaluateType,false,-1,-1);
        else
            RefleshPlayingUI(evaluateType,false,-1,-1);
        
        DestorySelf(false);
        
        Debug.Log($"Tap音符命中，评价:{evaluateType}，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
    }
}
