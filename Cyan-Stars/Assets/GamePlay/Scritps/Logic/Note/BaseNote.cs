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
    public bool IsDestroyed;

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

    public void AddMaxScore(int score)
    {
        GameManager.Instance.maxScore += score;
        Debug.Log($"最大得分：{GameManager.Instance.maxScore}");
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
        if (IsDestroyed)
        {
            return;
        }
    }
    
    /// <summary>
    /// 此音符所属轨道按键抬起
    /// </summary>
    public virtual void OnKeyUp()
    {
        if (IsDestroyed)
        {
            return;
        }
    }
    
    /// <summary>
    /// 此音符所属轨道按键按压中
    /// </summary>
    public virtual void OnKeyPress()
    {
        if (IsDestroyed)
        {
            return;
        }
    }


    /// <summary>
    /// 销毁自身
    /// </summary>
    protected void DestroySelf(bool autoMove = true,float destroyTime = 2f)
    {
        if (IsDestroyed)
        {
            return;
        }
        
        IsDestroyed = true;
        view.DestroySelf(autoMove,destroyTime);
        view = null;
    }
    protected void RefreshPlayingUI(EvaluateType type,bool suc,int score,int combo)
    {
        if(suc)
        {
            GameManager.Instance.score += score;
            GameManager.Instance.combo += combo;
        }
        else
        {
            GameManager.Instance.combo = 0;
        }
        GameManager.Instance.RefreshPlayingUI(GameManager.Instance.combo,GameManager.Instance.score,type.ToString());
    }
}
