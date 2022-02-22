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
    protected IView view;
    private bool lastIsTiggered;
    
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
        if(view != null && view.IsTiggered() && !lastIsTiggered)
        {
            OnKeyDown();
            lastIsTiggered = true;
        }
        if(view != null && view.IsTiggered())  
        {
            OnKeyPress();
            lastIsTiggered = true;
        }
        if(view != null && !view.IsTiggered() && lastIsTiggered)
        {
            OnKeyUp();
            lastIsTiggered = false;
        }
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
    protected void RefleshPlayingUI(EvaluateType type,bool suc,int score,int combo)
    {
        string str = "";
        if(type == EvaluateType.Exact)str = "Exact!!!";
        else if(type == EvaluateType.Great)str = "Great!!";
        else if(type == EvaluateType.Right)str = "Right!";
        else if(type == EvaluateType.Bad)str = "Bad/(-_-)\\";
        else if(type == EvaluateType.Miss)str = "Miss";
        GameManager.Instance.grade = str; 
        if(suc)
        {
            GameManager.Instance.score += score;
            GameManager.Instance.combo += combo;
        }
        else
        {
            GameManager.Instance.combo = 0;
        }
    }
}
