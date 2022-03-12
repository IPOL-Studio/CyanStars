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


    private int pressCount;
    private float pressTime;

    public override void Init(NoteData data, MusicTimeline.Layer layer)
    {
        base.Init(data, layer);

        holdLength = (data.HoldEndTime - data.StartTime) / 1000f;
        //hold结束时间点与长度相同
        holdCheckInputEndTime = -holdLength;
    }

    public override bool CanReceiveInput()
    {
        return LogicTimer <= EvaluateHelper.CheckInputStartTime && LogicTimer >= holdCheckInputEndTime;
    }

    public override void OnUpdate(float deltaTime, float noteSpeedRate)
    {
        base.OnUpdate(deltaTime, noteSpeedRate);

        if (pressCount > 0 && LogicTimer <= 0)
        {
            //只在音符区域内计算有效时间
            pressTime += deltaTime;
        }

        if (LogicTimer < holdCheckInputEndTime)
        {
            if (!headSucess)
            {
                //被漏掉了 miss
                Debug.LogError($"Hold音符miss：{data}");
                GameManager.Instance.maxScore += 2;
                GameManager.Instance.RefreshData(-1, -1, EvaluateType.Miss, float.MaxValue);
            }
            else
            {
                viewObject.DestroyEffectObj();
                value = pressTime / holdLength;

                EvaluateType et = EvaluateHelper.GetHoldEvaluate(value);
                Debug.LogError($"Hold音符命中，百分比:{value},评价:{et},{data}");
                GameManager.Instance.maxScore++;
                if(et == EvaluateType.Exact)
                    GameManager.Instance.RefreshData(0,1,et,float.MaxValue);
                else if(et == EvaluateType.Great)
                    GameManager.Instance.RefreshData(0,0.75f,et,float.MaxValue);
                else if(et == EvaluateType.Right)
                    GameManager.Instance.RefreshData(0,0.5f,et,float.MaxValue);
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
                    EvaluateType et = EvaluateHelper.GetTapEvaluate(LogicTimer);
                    if (et == EvaluateType.Bad || et == EvaluateType.Miss)
                    {
                        //头判失败直接销毁
                        DestroySelf(false);
                        Debug.LogError($"Hold头判失败,时间：{LogicTimer}，{data}");
                        GameManager.Instance.maxScore += 2;
                        GameManager.Instance.RefreshData(-1, -1, et, float.MaxValue);
                        return;
                    }

                    Debug.LogError($"Hold头判成功,时间：{LogicTimer}，{data}");
                    GameManager.Instance.maxScore++;
                    if(et == EvaluateType.Exact)
                        GameManager.Instance.RefreshData(1,1,et,LogicTimer);
                    else if(et == EvaluateType.Great)
                        GameManager.Instance.RefreshData(1,0.75f,et,LogicTimer);
                    else if(et == EvaluateType.Right)
                        GameManager.Instance.RefreshData(1,0.5f,et,LogicTimer);
                }

                //头判成功
                headSucess = true;
                if (pressCount == 0) viewObject.CreateEffectObj(data.Width);
                pressCount++;
                break;

            case InputType.Up:

                pressCount--;
                if (pressCount == 0) viewObject.DestroyEffectObj();
                break;
        }

    }
}