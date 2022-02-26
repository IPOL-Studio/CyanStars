using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音符基类
/// </summary>
public abstract class BaseNote
{
    /// <summary>
    /// 音符数据
    /// </summary>
    protected NoteData data;

    /// <summary>
    /// 此音符所属图层
    /// </summary>
    protected MusicTimeline.Layer layer;
    
    /// <summary>
    /// 剩余时间的计时器
    /// </summary>
    protected float timer;

    /// <summary>
    /// 视图层物体
    /// </summary>
    protected IView viewObject;
    
    
    
    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData(NoteData data,MusicTimeline.Layer layer)
    {
        this.data = data;
        this.layer = layer;
        timer = data.StartTime;
        viewObject = ViewHelper.CreateViewObject(data);
    }

    /// <summary>
    /// 是否可接收输入
    /// </summary>
    public virtual bool CanReceiveInput()
    {
        return timer <= EvaluateHelper.CheckInputStartTime && timer >= (EvaluateHelper.CheckInputEndTime - data.HoldEndTime);
    }

    /// <summary>
    /// 是否与指定范围有重合
    /// </summary>
    public virtual bool IsInRange(float min,float max)
    {
        //3种情况可能重合 1.最左侧在范围内 2.最右侧在范围内 3.中间部分在范围内
        bool result = (data.Pos >= min && data.Pos <= max)
                      || ((data.Pos + data.Width) >= min && (data.Pos + data.Width) <= max)
                      || (data.Pos <= min && (data.Pos + data.Width) >= max);

        return result;
    }
    
    public virtual void OnUpdate(float deltaTime,float noteSpeedRate)
    {
        timer -= deltaTime;
        
        viewObject.OnUpdate(deltaTime * noteSpeedRate);
    }

    /// <summary>
    /// 此音符有对应输入时
    /// </summary>
    public virtual void OnInput(InputType inputType)
    {
        //Debug.Log($"音符接收输入:输入类型{inputType},倒计时器:{timer},数据{data}");
    }

    /// <summary>
    /// 销毁自身
    /// </summary>
    protected void DestorySelf(bool autoMove = true)
    {
        layer.RemoveNote(this);
        viewObject.DestorySelf(autoMove);
        viewObject = null;
    }
    

}
