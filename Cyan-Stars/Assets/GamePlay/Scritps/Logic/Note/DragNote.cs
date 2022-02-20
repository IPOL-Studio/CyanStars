using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drag音符，在时间范围内按住按键就算命中
/// </summary>
public class DragNote : BaseNote
{
    private const float dragTime = 0.03f;
    
    public DragNote(int trackIndex, NoteData data) : base(trackIndex, data)
    {
    }

    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (timer < -dragTime)
        {
            DestorySelf();
            Debug.LogError($"Drag音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
    }

    public override void OnKeyPress()
    {
        base.OnKeyPress();

        if (timer <= dragTime && timer >= -dragTime)
        {
            DestorySelf(false);
            Debug.LogError($"Drag音符命中，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
    }
}
