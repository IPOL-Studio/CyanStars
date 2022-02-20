using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Click音符，短按
/// </summary>
public class ClickNote : BaseNote
{
    private bool isKeyDown;
    private float keyDownTime;
    
    public ClickNote(int trackIndex, NoteData data) : base(trackIndex, data)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        
        if (timer <= EvaluateHelper.CheckInputEndTime * 2) //一次短按click大概要250ms，不乘2的话来不及
        {
            //Miss了
            DestorySelf();
            Debug.LogError($"Click音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
    }

    public override void OnKeyDown()
    {
        base.OnKeyDown();
        
        if (timer <= EvaluateHelper.CheckInputStartTime)
        {
            isKeyDown = true;
            keyDownTime = timer;
        }
    }

    public override void OnKeyUp()
    {
        base.OnKeyUp();

        if (isKeyDown)
        {
            //总的按住时长
            float time = Mathf.Abs(keyDownTime - timer);
            
            DestorySelf(false);
            
            Debug.LogError($"Click音符命中，按住时长:{time}，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
    }
}
