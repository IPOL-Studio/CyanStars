using System.Collections;
using System.Collections.Generic;

namespace CatTimeline
{
    /// <summary>
    /// 时间轴片段基类
    /// </summary>
    public abstract class BaseClip
    {
        public BaseClip(float startTime, float endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
        
        /// <summary>
        /// 持有此片段的轨道
        /// </summary>
        public BaseTrack Owner
        {
            get;
            set;
        }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public float StartTime
        {
            get;
            private set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public float EndTime
        {
            get;
            private set;
        }


        /// <summary>
        /// 进入此片段时调用
        /// </summary>
        public virtual void OnEnter()
        {
            
        }

        /// <summary>
        /// 离开此片段时调用
        /// </summary>
        public virtual void OnExit()
        {
            
        }
        
        /// <summary>
        /// 更新片段
        /// </summary>
        public virtual void Update(float currentTime,float previousTime)
        {
            
        }
    }
}


