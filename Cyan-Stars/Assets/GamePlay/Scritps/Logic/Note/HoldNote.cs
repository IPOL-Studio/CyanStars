using UnityEngine;

/// <summary>
/// Hold音符，长按检测
/// </summary>
public class HoldNote : BaseNote
{
    /// <summary>
    /// 是否正确按下
    /// </summary>
    private bool isKeyDown;
    
    /// <summary>
    /// 正确按下时间
    /// </summary>
    private float keyDownTime;
    
    public HoldNote(int trackIndex, NoteData data) : base(trackIndex, data)
    {
    }
    
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        
        if (timer <= (EvaluateHelper.CheckInputEndTime - data.HoldLength))//因为是一长条 所以要加上长度
        {
            DestorySelf();
            Debug.LogError($"音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
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

            //按住时间百分比
            float result = time / data.HoldLength;
            EvaluateType evaluateType = EvaluateHelper.GetHoldEvaluate(result);
            DestorySelf(false);
            
            Debug.LogError($"Hold音符命中，按住时长:{time}，百分比:{result}，评价:{evaluateType}，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
        }
        
    }
}