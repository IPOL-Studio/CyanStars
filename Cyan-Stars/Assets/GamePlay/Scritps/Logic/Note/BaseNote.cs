using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音符基类
/// </summary>
public abstract class BaseNote
{
    /// <summary>
    /// 轨道编号
    /// </summary>
    private int trackIndex;
    
    /// <summary>
    /// 音符数据
    /// </summary>
    protected NoteData data;
    
    /// <summary>
    /// 剩余时间的计时器
    /// </summary>
    protected float timer;

    /// <summary>
    /// 是否已销毁
    /// </summary>
    public bool IsDestoryed;

    /// <summary>
    /// 视图层物体
    /// </summary>
    private IView view;
    
    public BaseNote(int trackIndex, NoteData data)
    {
        this.trackIndex = trackIndex;
        this.data = data;
        timer = data.TimePoint;
        
        view = GameMgr.Instance.CreateView(trackIndex,data);
    }
    
    public override string ToString()
    {
        return $"类型:{data.Type},轨道:{trackIndex}";
    }
    
    public virtual void OnUpdate(float deltaTime)
    {
        //需要考虑音符速率
        deltaTime *= data.SpeedRate;
        timer -= deltaTime;

        view.OnUpdate(deltaTime);
    }

    /// <summary>
    /// 此音符所属轨道按键按下
    /// </summary>
    public virtual void OnKeyDown()
    {
        if (IsDestoryed)
        {
            return;
        }
    }
    
    /// <summary>
    /// 此音符所属轨道按键抬起
    /// </summary>
    public virtual void OnKeyUp()
    {
        if (IsDestoryed)
        {
            return;
        }
    }
    
    /// <summary>
    /// 此音符所属轨道按键按压中
    /// </summary>
    public virtual void OnKeyPress()
    {
        if (IsDestoryed)
        {
            return;
        }
    }


    /// <summary>
    /// 销毁自身
    /// </summary>
    protected void DestorySelf(bool autoMove = true)
    {
        if (IsDestoryed)
        {
            return;
        }
        
        IsDestoryed = true;
        view.DestorySelf(autoMove);
        view = null;
    }
}
