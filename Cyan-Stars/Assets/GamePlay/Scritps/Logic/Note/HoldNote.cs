using System;
using UnityEngine;

/// <summary>
/// Hold音符
/// </summary>
public class HoldNote : BaseNote
{

    /// <summary>
    /// Hold音符的检查输入结束时间
    /// </summary>
    private float holdCheckInputEndTime;

    /// <summary>
    /// Hold音符长度
    /// </summary>
    private float holdLength;
    
    /// <summary>
    /// 头判是否成功
    /// </summary>
    private bool headSucess;
    
    /// <summary>
    /// 累计有效时长值(0-1)
    /// </summary>
    private float value;

    /// <summary>
    /// 按下的时间点
    /// </summary>
    //private float downTimePoint;
    //此处是Ybr的改动

    private int pressCount;//此处是Ybr的改动
    private float pressTime;//此处是Ybr的改动

    public override void Init(NoteData data, MusicTimeline.Layer layer)
    {
        base.Init(data, layer);

        holdLength = data.HoldEndTime - data.StartTime;
        //hold结束时间点要算上hold音符的长度
        holdCheckInputEndTime = EvaluateHelper.CheckInputEndTime - holdLength;
    }

    public override bool CanReceiveInput()
    {
        return logicTimer <= EvaluateHelper.CheckInputStartTime && logicTimer >= holdCheckInputEndTime;
    }

    public override void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        base.OnUpdate(deltaTime,noteSpeedRate);
        if(pressCount > 0)
        {
            pressTime += Time.deltaTime;
        }//此处是Ybr的改动
        if (logicTimer < holdCheckInputEndTime)
        {
            if (!headSucess)
            {
                //被漏掉了 miss
                Debug.LogError($"Hold音符miss：{data}");
                GameManager.Instance.RefreshData(-1,-1,"Miss",float.MaxValue);
            }
            else
            {
                /*
                if (downTimePoint != 0)
                {
                    //按下后一直持续到结尾的话 也要算一下分
                    float time = downTimePoint - logicTimer;
                    value += time / holdLength;
                    //Debug.LogError($"Hold音符分数：{value}");
                }
                */
                value = pressTime / holdLength;//此处是Ybr的改动
                
                EvaluateType et =  EvaluateHelper.GetHoldEvaluate(value);
                Debug.LogError($"Hold音符命中，百分比:{value},评价:{et},{data}");
                if(et != EvaluateType.Miss)
                {
                    GameManager.Instance.RefreshData(1,1,et.ToString(),float.MaxValue);
                }
                else
                {
                    GameManager.Instance.RefreshData(-1,-1,"Miss",float.MaxValue);
                }
            }
            
            DestroySelf();
        }
    }

    public override void OnInput(InputType inputType)
    {
        base.OnInput(inputType);

        switch (inputType)
        {
            case InputType.Down:

                if (!headSucess)
                {
                    //判断头判评价
                    EvaluateType et = EvaluateHelper.GetTapEvaluate(logicTimer);
                    if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                    {
                        //头判失败直接销毁
                        DestroySelf(false);
                        Debug.LogError($"Hold头判失败,时间：{logicTimer}，{data}");
                        GameManager.Instance.RefreshData(-1,-1,et.ToString(),float.MaxValue);
                        return;
                    }

                    Debug.LogError($"Hold头判成功,时间：{logicTimer}，{data}");
                    GameManager.Instance.RefreshData(1,1,et.ToString(),logicTimer);
                }
                
                //头判成功
                headSucess = true;
                //downTimePoint = logicTimer;
                pressCount ++;//此处是Ybr的改动
                break;

            case InputType.Up:
                /*
                if (downTimePoint != 0)
                {
                    //此次有效时长
                    float time = downTimePoint - logicTimer;
                    value += time / holdLength;
                    
                    //重置按下时间点
                    downTimePoint = 0;
                }*/
                pressCount --;//此处是Ybr的改动
                break;
        }
        
    }
}