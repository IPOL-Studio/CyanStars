using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public partial class MusicTimeline
{
    /// <summary>
    /// 节点基类
    /// </summary>
    public abstract class BaseNode
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public readonly float StartTime;

        /// <summary>
        /// 是否已执行结束
        /// </summary>
        public bool IsEnd
        {
            get;
            protected set;
        }
        
        protected BaseNode(float startTime)
        {
            StartTime = startTime;
        }
        
        public virtual void OnUpdate(float curTime,float deltaTime)
        {
        }
    }
}