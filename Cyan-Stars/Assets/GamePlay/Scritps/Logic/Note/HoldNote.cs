﻿using UnityEngine;

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
    /// <summary>
    /// 经过时间
    /// </summary>
    private float time;
    /// <summary>
    /// 头部判定
    /// </summary>
    private bool isClicked;
    
    public HoldNote(int trackIndex, NoteData data) : base(trackIndex, data)
    {
    }
    
    public override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);
        if(timer - data.HoldLength/4 < EvaluateHelper.CheckInputEndTime && !isClicked)
        {
            //Miss了
            DestorySelf();
            Debug.Log($"音符Miss，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}");
            RefleshPlayingUI(EvaluateType.Miss,false,-1,-1);
        }
        if(timer + data.HoldLength/4 < EvaluateHelper.CheckInputEndTime && isClicked)
        {
            float result = keyDownTime/time;
            EvaluateType evaluateType = EvaluateHelper.GetHoldEvaluate(result);
            Debug.Log($"Hold音符命中，评价:{evaluateType}，时间轴时间：{GameMgr.Instance.GetCurTimelineTime()},{this}，按住时间比例：{result}");
            if(evaluateType == EvaluateType.Exact || evaluateType == EvaluateType.Great || evaluateType == EvaluateType.Right)
                RefleshPlayingUI(evaluateType,true,1,1);
            else
                RefleshPlayingUI(evaluateType,false,-1,-1);
            DestorySelf();
        }
        if(timer <= data.HoldLength/4 && timer >= (EvaluateHelper.CheckInputEndTime - data.HoldLength/4))
        {
            time += Time.deltaTime;
        }
    }
    public override void OnKeyPress()
    {
        base.OnKeyPress();
        if(timer <= data.HoldLength/4 && timer >= (EvaluateHelper.CheckInputEndTime - data.HoldLength/4))
        {
            isKeyDown = true;
            keyDownTime += Time.deltaTime;
            view.GetTransform().GetChild(0).gameObject.SetActive(true);
            //Debug.Log("Yes" + (EvaluateHelper.CheckInputEndTime - data.HoldLength/4) + " " + timer + " " + data.HoldLength/4);
        }
        else    
        {
            isKeyDown = false;
            view.GetTransform().GetChild(0).gameObject.SetActive(false);
            //Debug.Log("No" + (EvaluateHelper.CheckInputEndTime - data.HoldLength/4) + " " + timer + " " + data.HoldLength/4);
        }
    }

    public override void OnKeyDown()
    {
        base.OnKeyDown();
        if(timer - data.HoldLength/4 <= EvaluateHelper.CheckInputStartTime && timer - data.HoldLength/4 >= EvaluateHelper.CheckInputEndTime)
        {
            isClicked = true;
            EvaluateType evaluateType = EvaluateHelper.GetHoldEvaluate(timer);
            RefleshPlayingUI(evaluateType,true,1,1);
        }
    }
}